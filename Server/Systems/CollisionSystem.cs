using Arch.Core;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics;
using Hypercube.Physics.Shapes.Structs;
using Hypercube.Physics.Manifolds;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.Components.Commands;
using Shared.Data;

namespace Server.Systems;

[EcsSystem(EcsPriority.UpdateCollisions)]
public class CollisionSystem : BaseSystem
{
    [Dependency] private readonly CollisionWorldSystem _worldSystem = null!;
    private readonly List<Entity> _neighborBuffer = new(32);
    
    private readonly QueryDescription _movableQuery = new QueryDescription()
        .WithAll<NetworkTransform, HitboxComponent>();

    public override void Update(long tick)
    {
        world.Query(in _movableQuery, (Entity entity, ref NetworkTransform trans, ref HitboxComponent hitbox) =>
        {
            if (hitbox.IsStatic || hitbox.IsTrigger || !hitbox.GridIndex.HasValue) 
                return;

            _neighborBuffer.Clear();
            _worldSystem.GetNearby(hitbox.GridIndex.Value, _neighborBuffer);
            
            foreach (var neighbor in _neighborBuffer)
            {
                if (entity == neighbor) 
                    continue;

                ResolveCollision(entity, neighbor);
            }
        });
    }

    public Manifold ResolveCollision(Entity entityA, Entity entityB)
    {
        ref var transformA = ref world.Get<NetworkTransform>(entityA);
        ref var transformB = ref world.Get<NetworkTransform>(entityB);
        
        ref var hitboxA = ref world.Get<HitboxComponent>(entityA);
        ref var hitboxB = ref world.Get<HitboxComponent>(entityB);
        
        if (hitboxA.IsTrigger || hitboxB.IsTrigger)
            return Manifold.Empty;
        
        var manifold = GetManifold(ref transformA, ref hitboxA, ref transformB, ref hitboxB);
        if (manifold.IsEmpty)
            return manifold;
        
        var pushFactor = hitboxB.IsStatic ? 1.0f : 0.5f;
        var maxSeparation = 0f;
        for (var i = 0; i < manifold.PointCount; i++)
        {
            var point = manifold.Points[i];
            if (point.Separation < maxSeparation)
            {
                maxSeparation = point.Separation;
            }
        }
        
        var correction = manifold.Normal * (maxSeparation * pushFactor);
        if (correction == Vector2.Zero)
            return Manifold.Empty;
        
        if (!hitboxB.IsStatic)
            transformB.Position -= correction;
        
        Console.WriteLine(2);
        Console.WriteLine(correction);
        transformA.Position += correction;
        return manifold;
    }
    
    private Manifold GetManifold(ref NetworkTransform tA, ref HitboxComponent hA, ref NetworkTransform tB, ref HitboxComponent hB)
    {
        var physicsTransA = new Transform(tA.Position);
        var physicsTransB = new Transform(tB.Position);

        return Contacts.Resolve(hA.Shape, physicsTransA, hB.Shape, physicsTransB);
    }
    
    public bool HasOverlap(Entity entity)
    {
        ref var transformA = ref world.Get<NetworkTransform>(entity);
        ref var hitbox = ref world.Get<HitboxComponent>(entity);
        
        var results = new List<Entity>();
        _worldSystem.GetNearby(hitbox.GridIndex!.Value, results);

        foreach (var other in results)
        {
            var manifold = ResolveCollision(entity, other);
            if (manifold.IsEmpty)
                continue;
            
            return true;
        }

        return false;
    }
    
    public Manifold GetFirstOverlap(Entity entity)
    {
        ref var transformA = ref world.Get<NetworkTransform>(entity);
        ref var hitbox = ref world.Get<HitboxComponent>(entity);
        
        var results = new List<Entity>();
        _worldSystem.GetNearby(hitbox.GridIndex!.Value, results);

        foreach (var other in results)
        {
            var manifold = ResolveCollision(entity, other);
            if (manifold.IsEmpty)
                continue;
            
            return manifold;
        }

        return Manifold.Empty;
    }
}