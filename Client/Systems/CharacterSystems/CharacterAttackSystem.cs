using Client.Data;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Core.Input.Handler;
using Hypercube.Core.Viewports;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Components.Requests;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Client.Systems.CharacterSystems;

[EcsSystem]
public class CharacterAttackSystem : BaseSystem
{
    [Dependency] private readonly InputStorage _inputStorage = null!;
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    [Dependency] private readonly NetworkHelper _networkHelper = null!;
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly ICameraManager _cameraManager = null!;
    
    private readonly QueryMeta _meta = new QueryMeta().WithAll<PlayerCharacter, NetworkTransform>();

    public override void GameUpdate(long serverTick, long predictTick)
    {
        Query(_meta).With<PlayerCharacter, NetworkTransform>((entity, ref playerCharacter, ref transform) =>
        {
            if (_gameClient.Id != playerCharacter.ClientId)
                return;

            var camera = _cameraManager.MainCamera;
            var worldPosition = camera.ScreenToWorld(_inputHandler.MousePosition).Xy;
            var direction = (worldPosition - transform.Position).Normalized;
            
            if (_inputStorage.HasInput(Input.Attack))
            {
                _networkHelper.SendInputIfPredicting(new AttackRequest { Direction = direction }, DeliveryMethod.Unreliable);
            }
        });
    }
}