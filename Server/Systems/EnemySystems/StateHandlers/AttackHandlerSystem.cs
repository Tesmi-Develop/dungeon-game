using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Server.Extensions;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems.StateHandlers;

[EcsSystem]
public class AttackHandlerSystem : BaseSystem
{
    [Dependency] private readonly AnimatorSystem _animatorSystem = null!;
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<
        State, 
        Animator, 
        NetworkTransform, 
        CollisionComponent, 
        Target, 
        AttackInfo
    >();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<State, Animator>((entity, ref state, ref animator) =>
        {
            if (state.StateType != StateType.Attacking)
                return;

            state.FrozenState = true;
            if (!animator.IsPlaying)
            {
                state.FrozenState = false;
                state.StateType = StateType.Idle;
                return;
            }

            if (_animatorSystem.IsEventTriggered(entity, "Hit"))
            {
                var target = GetComponent<Target>(entity);
                var transform = GetComponent<NetworkTransform>(entity);
                var collision = GetComponent<CollisionComponent>(entity);
                var attackInfo = GetComponent<AttackInfo>(entity);
                
                if (!target.TargetEntity.HasValue)
                    return;
                
                var direction = (GetComponent<NetworkTransform>(target.TargetEntity!.Value).Position - transform.Position).Normalized;
                var radius = attackInfo.AttackSize.X / 2 + collision.Size.X / 2;
                
                World.CreateDamageableCollision(new WorldExtensions.CollisionInfo
                {
                    Position = transform.Position + direction * radius,
                    Size = attackInfo.AttackSize,
                    Rotation = direction.AsAngle(),
                }, new WorldExtensions.DamagePayload
                {
                    Damage = attackInfo.Damage
                });
            }
        });
    }
}