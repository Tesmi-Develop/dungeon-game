using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Helpers;
using Server.Utilities;
using Shared.Components.EngineComponents;
using Shared.Components.Requests;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.PlayerSystems;

[EcsSystem]
public class PlayerAttackSystem : BaseSystem
{
    private readonly QueryMeta _meta = new QueryMeta().WithAll<ClientData>();
        
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_meta).With((Entity clientEntity, ref ClientData _) =>
        {
            if (!World.Has<ControlledEntity>(clientEntity))
                return;
            
            var characterEntity = World.Get<ControlledEntity>(clientEntity).Reference;
            if (!World.Validate(characterEntity))
                return;
            
            if (HasComponent<Attacking>(characterEntity) || !HasComponent<NetworkTransform>(characterEntity))
                return;

            if (!NetworkHelper.TryGetInputFromTick<AttackRequest>(World, clientEntity, tick, out var inputData))
                return;
            
            ref var transform = ref World.Get<NetworkTransform>(characterEntity);

            World.SetState(characterEntity, new Attacking { TargetPosition = transform.Position + inputData.Direction });
        });
    }
}