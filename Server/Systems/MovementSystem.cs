using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Helpers;
using Server.Utilities;
using Shared.Components;
using Shared.Components.Commands;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem]
public class MovementSystem : BaseSystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<ClientData>().Build();
    }

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
            
            if (!World.Has<NetworkTransform>(characterEntity) || !World.Has<Speed>(characterEntity))
                return;
            
            ref var transform = ref World.Get<NetworkTransform>(characterEntity);
            ref var speed = ref World.Get<Speed>(characterEntity);
            
            transform.Position += inputData.Direction * speed.Value;
            NetworkHelper.MakeDirty<NetworkTransform>(World, characterEntity);
        });
    }
}