using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.Attributes.Engine;
using Shared.SharedSystemRealisation;

namespace Server.Systems.UtilitySystems;

[EcsSystem]
public class DestroySystem : BaseSystem
{
    private QueryMeta _query = new QueryMeta().WithAll<DestroyTag>();

    [Priority(EcsPriority.Low)]
    public override void AfterGameUpdate(long tick, long predictTick)
    {
        ForEach(_query, EntityDestroy);
    }
}