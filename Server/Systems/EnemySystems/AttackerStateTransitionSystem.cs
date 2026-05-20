using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components.Events;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.Enemies.EnemyTags;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class AttackerStateTransitionSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Target, State, AttackerTag, EnemyTag>();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<Target, State>((entity, ref target, ref state) =>
        {
            if (state.FrozenState)
                return;
            
            if (!target.TargetEntity.HasValue)
            {
                SetState(entity, ref state, StateType.Idle);
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
                    SetState(entity, ref state, StateType.Attacking);
                    return;
                }
            }
            
            if (HasComponent<NetworkTransform>(entity))
            {
                SetState(entity, ref state, StateType.Moving);
                return;
            }
        });
    }

    private void SetState(Entity entity, ref State state, StateType stateType)
    {
        var prevValue = state.PrevStateType;
        state.StateType = stateType;
        
        if (state.StateType == prevValue)
            return;
        
        NetworkHelper.MakeDirty<State>(World, entity);
        Raise(entity, ref state, new StateUpdated());
    }
}