using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Server.Extensions;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.EngineComponents;
using Shared.Components.States;
using Shared.Data;
using Shared.Extensions;
using Shared.SharedSystemRealisation;
using WorldExtensions = Server.Extensions.WorldExtensions;

namespace Server.Systems.EnemySystems.StateHandlers;

[EcsSystem]
public class AttackHandlerSystem : BaseSystem
{
    [Dependency] private readonly AnimatorSystem _animatorSystem = null!;
    private readonly List<Entity> _entities = [];
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<
        Attacking,
        State,
        Animator, 
        NetworkTransform, 
        CollisionComponent,
        AttackInfo
    >();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        foreach (var entity in World.CollectEntities(_queryMeta, _entities))
        {
            ref var attackingState = ref GetComponent<Attacking>(entity);
            ref var animator = ref GetComponent<Animator>(entity);

            if (!_animatorSystem.IsEventTriggered(entity, "Hit")) 
                continue;
            
            var transform = GetComponent<NetworkTransform>(entity);
            var collision = GetComponent<CollisionComponent>(entity);
            var attackInfo = GetComponent<AttackInfo>(entity);
            var fractionValue = HasComponent<Fraction>(entity) ? GetComponent<Fraction>(entity).Value : FractionType.Players;
                
            var direction = (attackingState.TargetPosition - transform.Position).Normalized;
            var radius = attackInfo.AttackSize.X / 2 + collision.Size.X / 2;
                
            World.CreateDamageableCollision(new WorldExtensions.CollisionInfo
            {
                Position = transform.Position + direction * radius,
                Size = attackInfo.AttackSize,
                Rotation = direction.AsAngle(),
            }, new WorldExtensions.DamagePayload
            {
                Damage = attackInfo.Damage
            }, fractionValue);
        }
    }
}