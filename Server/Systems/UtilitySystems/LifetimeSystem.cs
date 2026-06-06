using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.Attributes.Engine;
using Shared.SharedSystemRealisation;

namespace Server.Systems.UtilitySystems;

[EcsSystem]
public class LifetimeSystem : BaseSystem
{
    private QueryMeta _queryMeta = new QueryMeta().WithAll<Lifetime>().WithNone<DeferredTag>();
    
    [Priority(EcsPriority.Low)]
    public override void AfterGameUpdate(long tick, long predictTick)
    {
        With<Lifetime>(_queryMeta, (entity, ref lifetimeComponent) =>
        {
            lifetimeComponent.RemainingTicks--;

            if (lifetimeComponent.RemainingTicks <= 0)
                EntityDestroy(entity);
        });
    }
}