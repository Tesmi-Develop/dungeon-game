using Client.Data;
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
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem, Scene(SceneType.Game)]
public class LightSystem : BaseSystem, IPatch
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    [Dependency] private readonly IRenderContext _renderContext = null!;
    [Dependency] private readonly ICameraManager _cameraManager = null!;

    public float DarkAlpha = 1f;
    private QueryMeta _meta = new QueryMeta().WithAll<Light, NetworkTransform>();
    private Shader _circle_light_shader = null!;
    private Shader _rectangle_light_shader = null!;
    private Surface _surface = default;
    
    public int Priority => -9;

    public override void Initialize()
    {
        _circle_light_shader = _resourceManager.Load<Shader>("/Shaders/circle_light.shd");
        _rectangle_light_shader = _resourceManager.Load<Shader>("/Shaders/rectangle_light.shd");
        _surface = _renderContext.CreateSurface(new Vector2i(4000, 4000));
    }

    private Shader ResolveLightShader(IRenderContext renderer, ref Light light)
    {
        if (light.LightType == LightType.Circle)
        {
            renderer.SetShader(_circle_light_shader);
            return _circle_light_shader;
        }
        
        if (light.LightType == LightType.Rectangle)
        {
            renderer.SetShader(_rectangle_light_shader);
            return _rectangle_light_shader;
        }
        
        throw new NotSupportedException($"Unsupported lighttype {light.LightType}");
    }
    
    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        _surface.Color = Color.Black.WithA(DarkAlpha);
        renderer.BindSurface(_surface);
        
        using (renderer.UseRenderState(_surface))
        {
            Query(_meta).With<Light, NetworkTransform>((entity, ref light, ref transform) =>
            {
                var shader = ResolveLightShader(renderer, ref light);
                var size = light.LightType == LightType.Circle ? new Vector2(light.Radius * 2) : light.Size;
                
                shader.SetUniform("intensity", light.Intensity);
                shader.SetUniform("falloff", light.Falloff);
                shader.SetUniform("maskSize", size);

                var finalPosition = transform.Position;
                var leftPosition = finalPosition.X - size.X / 2;
                var topPosition = finalPosition.Y + size.Y / 2;
                var rightPosition = finalPosition.X + size.X / 2;
                var bottomPosition = finalPosition.Y - size.Y / 2;
            
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
                renderer.ClearShader();
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