using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Systems;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;

namespace Rainclipse;

public sealed class TestPathSystem : PatchEntitySystem
{
    [Dependency] private readonly IResourceManager _resource = null!;
    
    private Query _transformQuery = null!;
    private Font _font = null!;

    public override void Initialize()
    {
        _transformQuery = GetQuery()
            .WithAll<TransformComponent>()
            .Build();
        
        _font = _resource.Load<Font>("/fonts/OpenSans.ttf", [("size", 24)]);
    }
    
    public override void Draw(IRenderContext renderer, DrawPayload payload)
    {
        _transformQuery.With<TransformComponent>((entity, ref transform) =>
        {
            var transformString = string.Empty;
            transformString += $"Pos: {transform.LocalPosition}\r\n";
            transformString += $"Rot: {transform.LocalRotation} ({transform.LocalRotation.ToEulerDeg()})\r\n";
            transformString += $"Scl: {transform.LocalScale}";
            
            var position = transform.LocalPosition + new Vector2(32, 32);
            renderer.DrawText(transformString, _font, position.Xy, Color.White);
        });
    }
}