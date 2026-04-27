using Arch.Core;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Server.Events;
using Server.Helpers;
using Shared.Components;

namespace Server.Systems;

[EcsSystem]
public class TestSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    private QueryDescription _query;
    private QueryDescription _query2;
    private int counter = 0;
    
    public override void PostInitialize()
    {
        var entity2 = world.Create();
        world.Add(entity2, new Transform() { Position = Vector2.One });
        _query = new QueryDescription().WithAll<Transform>();
    }

    public override void Update(float deltaTime)
    {
        world.Query(in _query, (Entity entity, ref Transform component) =>
        {
            component.Position += Vector2.One * deltaTime;
            NetworkHelper.MakeDirty<Transform>(world, entity);
        });
    }
}