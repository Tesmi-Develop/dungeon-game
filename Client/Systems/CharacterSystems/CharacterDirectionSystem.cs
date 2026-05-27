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
public class CharacterDirectionSystem : BaseSystem
{
    [Dependency] private readonly InputStorage _inputStorage = null!;
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    [Dependency] private readonly NetworkHelper _networkHelper = null!;
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly ICameraManager _cameraManager = null!;
    
    private readonly QueryMeta _meta = new QueryMeta().WithAll<PlayerCharacter, NetworkTransform, Animator>();

    public override void GameUpdate(long serverTick, long predictTick)
    {
        Query(_meta).With<PlayerCharacter, NetworkTransform, Animator>((entity, ref playerCharacter, ref transform, ref animator) =>
        {
            if (_gameClient.Id != playerCharacter.ClientId)
                return;

            var camera = _cameraManager.MainCamera;
            var worldPosition = camera.ScreenToWorld(_inputHandler.MousePosition).Xy;
            var direction = (worldPosition - transform.Position).Normalized;
            var finalDirection = new Vector2(direction.X > 0 ? 1 : -1, 1);
            
            if (finalDirection == animator.Scale)
                return;
            
            animator.Scale = finalDirection;
            _networkHelper.SendInputIfPredicting(new SetRotation() { Sign = (int)finalDirection.X }, DeliveryMethod.Unreliable);
        });
    }
}