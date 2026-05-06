using Arch.Core;
using Server.Components;
using Server.Helpers;
using Shared.Components;
using Shared.Components.Commands;

namespace Server.Systems;

[EcsSystem]
public class MovementSystem : BaseSystem
{
    private QueryDescription _query;

    public override void Initialize()
    {
        _query = new QueryDescription().WithAll<ClientData>();
    }

    public override void Update(long tick)
    {
        world.Query(in _query, (Entity clientEntity, ref ClientData clientData) =>
        {
            if (!world.Has<ControlledEntity>(clientEntity))
                return;
            
            var characterEntity = world.Get<ControlledEntity>(clientEntity).Reference;
            if (!world.IsAlive(characterEntity))
                return;

            if (!NetworkHelper.TryGetInputFromTick<MoveRequest>(world, clientEntity, tick, out var inputData))
                return;
            
            ref var transform = ref world.TryGetRef<NetworkTransform>(characterEntity, out var hasTransform);
            ref var speed = ref world.TryGetRef<Speed>(characterEntity, out var hasSpeed);
            
            if (!hasTransform || !hasSpeed)
                return;
            
            transform.Position += inputData.Direction * speed.Value;
            NetworkHelper.MakeDirty<NetworkTransform>(world, characterEntity);
            Console.WriteLine(1);
        });
    }
}