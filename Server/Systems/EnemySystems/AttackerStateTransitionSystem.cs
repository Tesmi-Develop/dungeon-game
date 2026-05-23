using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components.Enemies;
using Shared.Components.Enemies.EnemyTags;
using Shared.Components.EngineComponents;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class AttackerStateTransitionSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Target, State, AttackerTag, EnemyTag>();
    private readonly List<Entity> _entities = [];

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        foreach (var entity in World.CollectEntities(_queryMeta, _entities))
        {
            ref var target = ref GetComponent<Target>(entity);
            ref var state = ref GetComponent<State>(entity);
            
            if (state.FrozenState)
                return;
            
            if (!target.TargetEntity.HasValue)
            {
                World.SetState<Idle>(entity);
                return;
            }

            if (!HasComponent<NetworkTransform>(entity))
                return;

            var myPosition = GetComponent<NetworkTransform>(entity).Position;
            
            if (HasComponent<AttackInfo>(entity))
            {
                var attackRange = GetComponent<AttackInfo>(entity).MaxTargetRange;
                var targetPosition = GetComponent<NetworkTransform>(target.TargetEntity.Value).Position;
                if (targetPosition.Distance(myPosition) <= attackRange)
                {
                    World.SetState(entity, new Attacking { TargetPosition = targetPosition });
                    return;
                }
            }
            
            if (HasComponent<NetworkTransform>(entity))
            {
                World.SetState<Moving>(entity);
                return;
            }
        }
    }
}