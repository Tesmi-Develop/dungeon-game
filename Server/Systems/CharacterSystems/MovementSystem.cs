using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.CharacterSystems;

[EcsSystem]
public class MovementSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<NetworkTransform, MovingDirection, Speed>();
    private readonly List<Entity> _entities = [];

    [Priority(EcsPriority.BeforeApplyDirection - 1)]
    public override void GameUpdate(long tick, long _)
    {
        foreach (var entity in World.CollectEntities(_queryMeta, _entities))
        {
            ref var transform = ref GetComponent<NetworkTransform>(entity);
            ref var direction = ref GetComponent<MovingDirection>(entity);
            ref var speed = ref GetComponent<Speed>(entity);
            
            if (direction.Direction == Vector2.Zero)
                continue;
            
            transform.Position += direction.Direction * speed.Value;
            direction.Direction = Vector2.Zero;
            NetworkHelper.MakeDirty<NetworkTransform>(World, entity);
        }
    }
}