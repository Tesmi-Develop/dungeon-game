using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem]
public class DestroySystem : BaseSystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<DestroyTag>().Build();
    }

    [Priority(EcsPriority.Low)]
    public void AfterUpdate(long tick)
    {
        _query.ForEach(entity =>
        {
            World.Delete(entity);
        });
    }
}