using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics;
using Hypercube.Physics.Collision;
using Hypercube.Physics.Manifolds;
using Hypercube.Physics.Mathematics;
using Hypercube.Utilities.Dependencies;
using Shared.Attributes;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Shared.Systems.Collisions;

[EcsSystem]
public class CollisionSystem : SharedSystem
{
    [Dependency] private readonly CollisionWorldSystem _worldSystem = null!;
    private readonly List<Entity> _neighborBuffer = new(32);

    private Query _movableQuery = null!;

    [Priority(EcsPriority.UpdateCollisions)]
    public override void GameUpdate(long tick, long _)
    {
        _movableQuery = GetQuery().WithAll<NetworkTransform, HitboxComponent>().Build();
        _movableQuery.With((Entity entity, ref NetworkTransform trans, ref HitboxComponent hitbox) =>
        {
            if (hitbox.IsStatic || !hitbox.GridIndex.HasValue) 
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
        ref var transformA = ref World.Get<NetworkTransform>(entityA);
        ref var transformB = ref World.Get<NetworkTransform>(entityB);
        
        ref var hitboxA = ref World.Get<HitboxComponent>(entityA);
        ref var hitboxB = ref World.Get<HitboxComponent>(entityB);
        
        if (hitboxB.IsTrigger)
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
        ref var transformA = ref World.Get<NetworkTransform>(entity);
        ref var hitbox = ref World.Get<HitboxComponent>(entity);
        
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
        ref var transformA = ref World.Get<NetworkTransform>(entity);
        ref var hitbox = ref World.Get<HitboxComponent>(entity);
        
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