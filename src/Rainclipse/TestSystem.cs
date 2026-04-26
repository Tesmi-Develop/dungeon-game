using Hypercube.Core.Audio.Manager;
using Hypercube.Core.Audio.Resources;
using Hypercube.Core.Ecs;
using Hypercube.Core.Input;
using Hypercube.Core.Input.Handler;
using Hypercube.Core.Resources;
using Hypercube.Core.Systems.Transform;
using Hypercube.Core.Viewports;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Quaternions;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using JetBrains.Annotations;

namespace Rainclipse;

[UsedImplicitly]
public sealed class TestSystem : EntitySystem
{
    [Dependency] private readonly IAudioManager _audio = null!;
    [Dependency] private readonly ICameraManager _camera = null!;
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    [Dependency] private readonly IResourceManager _resource = null!;

    private Query _testQuery = null!;
    private Query _test2Query = null!;
    
    public override void Initialize()
    {
        _testQuery = GetQuery()
            .WithAll<TransformComponent, TestComponent, Test2Component>()
            .Build();

        _test2Query = GetQuery()
            .WithAll<TestComponent>()
            .Build();
    }
    
    public override void Update(float deltaTime)
    {
        _testQuery.With<TransformComponent, TestComponent>((_, ref transform, ref _) =>
        {
            var dt = deltaTime;
            if (_inputHandler.IsKeyHeld(Key.LeftShift))
                dt /= 100;
                
            if (_inputHandler.IsKeyHeld(Key.D))
                transform.LocalPosition += Vector3.UnitX * dt;

            if (_inputHandler.IsKeyHeld(Key.A))
                transform.LocalPosition -= Vector3.UnitX * dt;

            if (_inputHandler.IsKeyHeld(Key.W))
                transform.LocalPosition += Vector3.UnitY * dt;

            if (_inputHandler.IsKeyHeld(Key.S))
                transform.LocalPosition -= Vector3.UnitY * dt;
            
            if (_inputHandler.IsKeyHeld(Key.E))
                transform.LocalRotation *= Quaternion.FromEulerZ(-dt / 100);
            
            if (_inputHandler.IsKeyHeld(Key.Q))
                transform.LocalRotation *= Quaternion.FromEulerZ(dt / 100);
            
            _camera.MainCamera.Position = _camera.MainCamera.Position.WithXy(transform.LocalPosition);
        });
        
        _test2Query.With<TestComponent>((entity, ref _) =>
        {
            AddComponent<Test2Component>(entity);
        });
    }
}