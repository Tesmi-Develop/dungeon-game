using Arch.Core;
using EcsServer.Components;

namespace EcsServer.Systems;

[EcsSystem(EcsPriority.Low)]
public class DestroySystem : BaseSystem
{
    private QueryDescription _query;

    public override void Initialize()
    {
        _query = new QueryDescription().WithAll<DestroyTag>();
    }

    public override void AfterUpdate(float deltaTime)
    {
        world.Query(in _query, entity =>
        {
            world.Destroy(entity);
        });
    }
}