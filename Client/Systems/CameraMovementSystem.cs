using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Viewports;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class CameraMovementSystem : BaseSystem
{
    [Dependency] private readonly ICameraManager _camera = null!;
    [Dependency] private readonly GameClient _gameClient = null!;
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, PlayerCharacter>().Build();
    }

    [Priority(EcsPriority.Low)]
    public override void AfterUpdate(FrameEventArgs args)
    {
        _query.With<NetworkTransform, PlayerCharacter>((entity, ref transform, ref playerCharacter) =>
        {
            if (_gameClient.Id != playerCharacter.ClientId)
                return;
            
            _camera.MainCamera.Position = _camera.MainCamera.Position.WithXy(transform.Position);
        });
    }
}