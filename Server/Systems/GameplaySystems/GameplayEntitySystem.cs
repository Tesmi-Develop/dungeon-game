using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Utilities;
using Shared.Attributes;
using Shared.Attributes.Engine;
using Shared.Components;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.GameplaySystems;

[EcsSystem]
public class GameplayEntitySystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<GameplayTag>();
    
    [Priority(EcsPriority.High)]
    public override void BeforeInitialize()
    {
        var entity = EntityCreate();
        AddComponent<GameplayTag>(entity);
    }

    public Entity Get()
    {
        return World.GetFirstEntity(Query(_queryMeta));
    }
}