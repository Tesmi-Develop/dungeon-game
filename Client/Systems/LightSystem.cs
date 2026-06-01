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
using Hypercube.Mathematics.Matrices;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components.Enemies;
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
    private QueryMeta _meta = new QueryMeta().WithAll<Light, NetworkTransform>();
    private Shader _shader = null!;
    private Surface _surface = default;
    
    public int Priority => -9;

    public override void Initialize()
    {
        _shader = _resourceManager.Load<Shader>("/Shaders/light_mask.shd");
        _surface = _renderContext.CreateSurface(new Vector2i(4000, 4000));

        var lightEntity = EntityCreate();
        AddComponent(lightEntity, Light.Default with{ Color = Color.Black, ColorIntensity = 0.2f, Radius = 1000f });
        AddComponent(lightEntity, new NetworkTransform() { Position = Vector2.Zero });
    }

    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
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
            
                float time = (float)Environment.TickCount / 1000f;
                float hue = (time * 60f) % 360f; // 60f — скорость переливания (градусов/сек)
                light.Color = HsvToColor(hue, 1.0f, 1.0f);
            
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
    
    private Color HsvToColor(float h, float s, float v)
    {
        h = (h % 360f + 360f) % 360f;
        float hh = h / 60f;
        int i = (int)Math.Floor(hh);
        float f = hh - i;
        float p = v * (1f - s);
        float q = v * (1f - s * f);
        float t = v * (1f - s * (1f - f));

        float r, g, b;
        switch (i)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        // ⚠️ Адаптируйте под конструктор вашего Color:
        // Если принимает float (0.0 - 1.0):
        return new Color(r, g, b);
    
        // Если принимает byte (0 - 255), раскомментируйте строку ниже:
        // return new Color((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }
}