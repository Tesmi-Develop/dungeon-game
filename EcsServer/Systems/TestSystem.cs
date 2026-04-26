using System.Numerics;
using Arch.Core;
using EcsServer.Components;
using EcsServer.Components.Events;
using EcsServer.Events;
using EcsServer.Helpers;
using Hypercube.Utilities.Dependencies;

namespace EcsServer.Systems;

[EcsSystem]
public class TestSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus;
    private QueryDescription _query;
    private QueryDescription _query2;
    private int counter = 0;
    
    public override void PostInitialize()
    {
       /* var entity = world.Create();
        world.Add(entity, new ClientData());

        var entity2 = world.Create();
        world.Add(entity2, new Transform() { Position = Vector2.One });
        
        _eventBus.Raise<ClientData, NewEntityClient>(entity, new NewEntityClient());
        
        _query = new QueryDescription().WithAny<ClientData>();
        _query2 = new QueryDescription().WithAll<Transform>();*/
    }

    public override void Update(float deltaTime)
    {
        /*world.Query(in _query, (Entity _, ref PlayerData payload) =>
        {
           Console.WriteLine(payload.PendingPackets.Count);
        });
        
        world.Query(in _query2, (Entity entity, ref Transform transform) =>
        {
            if (counter > 10)
            {
                world.Remove<Transform>(entity);
                return;
            }
            transform.Position += new Vector2(1, 0);
            NetworkHelper.MakeDirty<Transform>(world, entity);
            counter++;
        });*/
    }
}