using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Components.Events;
using Server.Utilities;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Data;
using Shared.SharedSystemRealisation;
using Shared.Systems.Collisions;

namespace Server.Systems.GameSystems;

[EcsSystem]
public class HitboxSystem : BaseSystem
{
    [Dependency] private readonly CollisionSystem _collisionSystem = null!;
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Damage, CollisionComponent, NetworkTransform>().WithNone<DeferredTag>();

    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<Damage>((entity, ref damageComponent) =>
        {
            var collisions = _collisionSystem.GetAllOverlap(entity);
            var fraction = HasComponent<Fraction>(entity) ? GetComponent<Fraction>(entity).Value : (FractionType?)null;
            
            foreach (var (otherEntity, _) in collisions)
            {
                if (!HasComponent<Health>(otherEntity))
                    continue;
                
                if (HasComponent<Fraction>(otherEntity) && GetComponent<Fraction>(otherEntity).Value == fraction)
                    continue;
                
                Logger.Info("Take damage!");
                Raise(otherEntity, ref GetComponent<Health>(otherEntity), new TookDamage
                {
                    Value = damageComponent.Value
                });
            }
        });
    }
}