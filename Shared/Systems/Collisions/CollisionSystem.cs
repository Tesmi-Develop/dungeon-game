using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Collision;
using Hypercube.Physics.Manifolds;
using Hypercube.Physics.Mathematics;
using Hypercube.Utilities.Dependencies;
using Shared.Attributes;
using Shared.SharedSystemRealisation;
using CollisionComponent = Shared.Components.EngineComponents.CollisionComponent;
using NetworkTransform = Shared.Components.EngineComponents.NetworkTransform;

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
        _movableQuery = GetQuery().WithAll<NetworkTransform, CollisionComponent>().Build();
        _movableQuery.With((Entity entity, ref NetworkTransform trans, ref CollisionComponent collision) =>
        {
            if (collision.IsStatic || !collision.GridIndex.HasValue) 
                return;

            _neighborBuffer.Clear();
            _worldSystem.GetNearby(collision.GridIndex.Value, _neighborBuffer);
            
            foreach (var neighbor in _neighborBuffer)
            {
                if (entity == neighbor) 
                    continue;
                
                HandleCollision(entity, neighbor);
            }
        });
    }

    public Manifold ResolveCollision(Entity entityA, Entity entityB)
    {
        ref var transformA = ref World.Get<NetworkTransform>(entityA);
        ref var transformB = ref World.Get<NetworkTransform>(entityB);
        
        ref var hitboxA = ref World.Get<CollisionComponent>(entityA);
        ref var hitboxB = ref World.Get<CollisionComponent>(entityB);
        
        return GetManifold(ref transformA, ref hitboxA, ref transformB, ref hitboxB);
    }
    
    public Manifold HandleCollision(Entity entityA, Entity entityB)
    {
        ref var transformA = ref World.Get<NetworkTransform>(entityA);
        ref var transformB = ref World.Get<NetworkTransform>(entityB);
        ref var hitboxB = ref World.Get<CollisionComponent>(entityB);
        
        var manifold = ResolveCollision(entityA, entityB);
        
        if (hitboxB.IsTrigger || manifold.IsEmpty)
            return Manifold.Empty;
        
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
    
    private Manifold GetManifold(ref NetworkTransform tA, ref CollisionComponent hA, ref NetworkTransform tB, ref CollisionComponent hB)
    {
        var physicsTransA = new Transform(tA.Position, hA.Rotation);
        var physicsTransB = new Transform(tB.Position, hB.Rotation);

        return Contacts.Resolve(hA.Shape, physicsTransA, hB.Shape, physicsTransB);
    }
    
    public bool HasOverlap(Entity entity)
    {
        ref var transformA = ref World.Get<NetworkTransform>(entity);
        ref var hitbox = ref World.Get<CollisionComponent>(entity);
        
        var results = new List<Entity>();
        _worldSystem.GetNearby(hitbox.GridIndex ?? _worldSystem.WorldToGrid(transformA.Position), results);

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
        ref var hitbox = ref World.Get<CollisionComponent>(entity);
        
        var results = new List<Entity>();
        _worldSystem.GetNearby(hitbox.GridIndex ?? _worldSystem.WorldToGrid(transformA.Position), results);

        foreach (var other in results)
        {
            var manifold = ResolveCollision(entity, other);
            if (manifold.IsEmpty)
                continue;
            
            return manifold;
        }

        return Manifold.Empty;
    }
    
    public List<(Entity entity, Manifold manifold)> GetAllOverlap(Entity entity)
    {
        ref var transformA = ref World.Get<NetworkTransform>(entity);
        ref var hitbox = ref World.Get<CollisionComponent>(entity);
        
        var entities = new List<Entity>();
        var results = new List<(Entity, Manifold)>();
        _worldSystem.GetNearby(hitbox.GridIndex ?? _worldSystem.WorldToGrid(transformA.Position), entities);

        foreach (var other in entities)
        {
            var manifold = ResolveCollision(entity, other);
            if (manifold.IsEmpty)
                continue;
            
            results.Add((other, manifold));
        }

        return results;
    }
}