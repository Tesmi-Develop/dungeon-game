using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Server.Components;
using Server.Helpers;
using Server.Utilities;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Components.Requests;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.PlayerSystems;

[EcsSystem]
public class PlayerDirectionHandler : BaseSystem
{
    private readonly QueryMeta _meta = new QueryMeta().WithAll<ClientData>();
    
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_meta).With((Entity clientEntity, ref ClientData _) =>
        {
            if (!World.Has<ControlledEntity>(clientEntity))
                return;
            
            var characterEntity = World.Get<ControlledEntity>(clientEntity).Reference;
            if (!World.Validate(characterEntity) || !World.IsAliveCharacter(characterEntity))
                return;
            
            if (!HasComponent<Animator>(characterEntity))
                return;

            if (!NetworkHelper.TryGetInputFromTick<SetRotation>(World, clientEntity, tick, out var inputData))
                return;
            
            ref var animator = ref World.Get<Animator>(characterEntity);
            animator.Scale = new Vector2(1 * Math.Sign(inputData.Sign), 1);
            NetworkHelper.MakeDirty<Animator>(World, characterEntity);
        });
    }
}