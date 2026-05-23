using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;
using MoveRequest = Shared.Components.Requests.MoveRequest;

namespace Server.Systems.PlayerSystems;

[EcsSystem]
public class PlayerMovementSystem : BaseSystem
{
    private readonly QueryMeta _meta = new QueryMeta().WithAll<ClientData>();

    [Priority(EcsPriority.StateUpdater + 1)]
    public override void GameUpdate(long tick, long _)
    {
        With(_meta, (Entity clientEntity, ref ClientData clientData) =>
        {
            if (!World.Has<ControlledEntity>(clientEntity))
                return;
            
            var characterEntity = World.Get<ControlledEntity>(clientEntity).Reference;
            if (!World.Validate(characterEntity) || !World.IsAliveCharacter(characterEntity))
                return;

            if (!NetworkHelper.TryGetInputFromTick<MoveRequest>(World, clientEntity, tick, out var inputData))
                return;
            
            if (!World.Has<MovingDirection>(characterEntity))
                return;
            
            ref var movingDirection = ref World.Get<MovingDirection>(characterEntity);
            movingDirection.Direction = inputData.Direction;
            World.SetState<Moving>(characterEntity);
        });
    }
}