using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.UtilitiySystems;

[EcsSystem]
public class DeferredEntitiesSystem : BaseSystem
{
    private QueryMeta _queryMeta = new QueryMeta().WithAll<DeferredTag>();
    private List<Entity> _entities = [];

    [Priority(EcsPriority.High)]
    public override void BeforeGameUpdate(long tick, long predictTick)
    {
        foreach (var e in World.CollectEntities(Query(_queryMeta), _entities))
            RemoveComponent<DeferredTag>(e);
    }
}