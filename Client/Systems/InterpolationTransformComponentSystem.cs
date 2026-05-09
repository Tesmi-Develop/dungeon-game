using Client.Components;
using Client.Extensions;
using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class InterpolationTransformComponentSystem : BaseSystem
{
    [Dependency] private readonly GameClient _client = null!;
    private double _visualTickCursor = -1;

    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery()
            .WithAll<NetworkTransform, TransformComponent, Interpolation, SpriteComponent>()
            .Build();
    }

    public override void Update(FrameEventArgs deltaTime)
    {
        const double bufferTicks = 3.0;
        var targetTick = _client.GetServerTickDouble() - bufferTicks;
        
        if (_visualTickCursor < 0)
            _visualTickCursor = targetTick;
        
        var lerpFactor = deltaTime.Delta.Milliseconds * 5.0f; 
        _visualTickCursor = double.Lerp(_visualTickCursor, targetTick, lerpFactor);
        
        if (_visualTickCursor > targetTick + 10)
            _visualTickCursor = targetTick;
         
        var renderTick = _visualTickCursor;

        _query.With<NetworkTransform, TransformComponent, Interpolation>((entity, ref net, ref transform, ref interp) =>
        {
            if (interp.IsBypass)
            {
                transform.LocalPosition = new Vector3(net.Position.X, net.Position.Y, transform.LocalPosition.Z);
                return;
            }
            
            var snapshots = interp.Snapshots;
            while (snapshots.TryPeekNext(out var next) && next.Tick <= renderTick)
            {
                snapshots.Dequeue();
            }
            
            if (snapshots.Count < 2)
            {
                if (snapshots.Count != 1) 
                    return;
                
                var last = snapshots.Peek();
                transform.LocalPosition = new Vector3(last.Position.X, last.Position.Y, transform.LocalPosition.Z);
                return;
            }
            
            var from = snapshots.Peek();
            snapshots.TryPeekNext(out var to);
            
            double tickDelta = to.Tick - from.Tick;
            if (tickDelta <= 0) 
                return;

            var t = (float)((renderTick - from.Tick) / tickDelta);
            var targetPos = Vector2.Lerp(from.Position, to.Position, Math.Clamp(t, 0f, 1f));

            transform.LocalPosition = new Vector3(
                targetPos.X, 
                targetPos.Y, 
                transform.LocalPosition.Z
            );
        });
    }
}