using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem]
public class LifetimeSystem : BaseSystem
{
    private QueryMeta _queryMeta = new QueryMeta().WithAll<Lifetime>().WithNone<DeferredTag>();
    private List<Entity> _entities = [];
    
    [Priority(EcsPriority.Low)]
    public override void AfterGameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<Lifetime>((entity, ref lifetimeComponent) =>
        {
            lifetimeComponent.RemainingTicks--;

            if (lifetimeComponent.RemainingTicks <= 0)
                _entities.Add(entity);
        });

        foreach (var e in _entities)
            EntityDestroy(e);
    }
}