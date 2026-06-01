using Client.Components;
using Client.Utilities;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Api;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Viewports;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components.EngineComponents;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class LightSystem : BaseSystem, IPatch
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    [Dependency] private readonly IRenderContext _renderContext = null!;
    [Dependency] private readonly ICameraManager _cameraManager = null!;

    public float DarkAlpha = 0.3f;
    private QueryMeta _meta = new QueryMeta().WithAll<Light, NetworkTransform>();
    private Shader _shader = null!;
    private Surface _surface = default;
    
    public int Priority => -9;

    public override void Initialize()
    {
        _shader = _resourceManager.Load<Shader>("/Shaders/light_mask.shd");
        _surface = _renderContext.CreateSurface(new Vector2i(4000, 4000));
    }

    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        _surface.Color = Color.Black.WithA(DarkAlpha);
        
        renderer.SetShader(_shader);
        renderer.BindSurface(_surface);
        
        using (renderer.UseRenderState(_surface))
        {
            Query(_meta).With<Light, NetworkTransform>((entity, ref light, ref transform) =>
            {
                _shader.SetUniform("intensity", light.Intensity);
                _shader.SetUniform("falloff", light.Falloff);

                var finalPosition = transform.Position;
                var leftPosition = finalPosition.X - light.Radius;
                var topPosition = finalPosition.Y + light.Radius;
                var rightPosition = finalPosition.X + light.Radius;
                var bottomPosition = finalPosition.Y - light.Radius;
            
                renderer.SetBlendMode(BlendMode.Subtractive);
                renderer.DrawRectangle(
                    new Rect2(
                        payload.Camera.WorldToScreen(new Vector2(leftPosition, topPosition)),
                        payload.Camera.WorldToScreen(new Vector2(rightPosition, bottomPosition))
                    ),
                    Color.White
                );
                renderer.SetBlendMode(BlendMode.Additive);
                renderer.DrawRectangle(
                    new Rect2(
                        payload.Camera.WorldToScreen(new Vector2(leftPosition, topPosition)),
                        payload.Camera.WorldToScreen(new Vector2(rightPosition, bottomPosition))
                    ),
                    light.Color.WithA(light.ColorIntensity)
                );
            
            });
        }
        
        renderer.SetBlendMode(BlendMode.Alpha);
        renderer.ClearShader();
        renderer.UnbindSurface();
        
        using (renderer.UseRenderState(payload.Window))
        {
            renderer.DrawSurface(_surface, Color.White);
        }
    }
}