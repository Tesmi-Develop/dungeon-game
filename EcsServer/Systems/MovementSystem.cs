using Arch.Core;
using EcsServer.Components;
using EcsServer.Components.Commands;

namespace EcsServer.Systems;

[EcsSystem]
public class MovementSystem : BaseSystem
{
    private QueryDescription _query;

    public override void Initialize()
    {
        _query = new QueryDescription().WithAll<FromClient, MoveRequest>();
    }

    public override void Update(float deltaTime)
    {
        world.Query(in _query, (Entity _, ref FromClient fromClient, ref MoveRequest moveRequest) =>
        {
            if (!world.Has<ControlledEntity>(fromClient.PlayerEntity))
                return;
            
            var characterEntity = world.Get<ControlledEntity>(fromClient.PlayerEntity).Reference;
            if (!world.IsAlive(characterEntity))
                return;
            
            if (
                !world.TryGet<Transform>(characterEntity, out var transform) || 
                !world.TryGet<Speed>(characterEntity, out var speed)
                )
                return;
            
            transform.Position += moveRequest.Direction * speed.Value;
        });
    }
}