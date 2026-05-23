using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Commands;
using Shared.SharedSystemRealisation;

namespace Server.Systems.PlayerSystems;

[EcsSystem]
public class PlayerMovementSystem : BaseSystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<ClientData>().Build();
    }

    [Priority(EcsPriority.StateUpdater + 1)]
    public override void GameUpdate(long tick, long _)
    {
        _query.With((Entity clientEntity, ref ClientData clientData) =>
        {
            if (!World.Has<ControlledEntity>(clientEntity))
                return;
            
            var characterEntity = World.Get<ControlledEntity>(clientEntity).Reference;
            if (!World.Validate(characterEntity))
                return;

            if (!NetworkHelper.TryGetInputFromTick<MoveRequest>(World, clientEntity, tick, out var inputData))
                return;
            
            if (!World.Has<MovingDirection>(characterEntity))
                return;
            
            ref var movingDirection = ref World.Get<MovingDirection>(characterEntity);
            movingDirection.Direction = inputData.Direction;
        });
    }
}