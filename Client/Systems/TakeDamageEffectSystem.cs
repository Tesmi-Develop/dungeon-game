using System.Diagnostics;
using Client.Utilities;
using GTweens.Builders;
using GTweens.Easings;
using GTweens.Enums;
using GTweens.Extensions;
using GTweens.Tweens;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components.Enemies;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class TakeDamageEffectSystem : BaseSystem, IPatch
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    public int Priority => 0;
    private Shader _shader = null!;
    private GTween? _tween;
    private float _intensity;
    private float _targetIntensity;

    public override void Initialize()
    {
        _shader = _resourceManager.Load<Shader>("/Shaders/vignette.shd");
    }

    private void PrepareTween()
    {
        if (_tween is not null && _tween.IsPlaying)
            _tween.Kill();
        
        _tween = GTweenSequenceBuilder
            .New()
            .Append(GTweenExtensions.Tween(
                () => _intensity,
                x => _intensity = x,
                () => _targetIntensity,
                0.15f,
                ValidationExtensions.AlwaysValid
            ).SetEasing(Easing.OutExpo))
            .Append(GTweenExtensions.Tween(
                () => _targetIntensity,
                x => _intensity = x,
                0,
                1f,
                ValidationExtensions.AlwaysValid
            ).SetEasing(Easing.OutSine))
            .Build();

        _tween.OnCompleteOrKill(() =>
        {
            _tween = null;
            _targetIntensity = 0;
        });
    }
    
    public void Invoke(float intensity)
    {
        intensity = Math.Clamp(intensity, 0, 1);
            
        if (intensity > _targetIntensity)
        {
            _targetIntensity = intensity;
            PrepareTween();
            _tween!.Start();
        }
    }

    public override void Update(FrameEventArgs args)
    {
        _tween?.Tick((float)args.Delta.TotalSeconds);
    }

    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        if (_intensity <= 0)
            return;
        
        using (renderer.UseRenderState(payload.Window))
        {
            _shader.Use();
            _shader.SetUniform("intensity", _intensity);
            _shader.Stop();
            renderer.DrawRectangle(Rect2.FromSize(Vector2.Zero, payload.Window.Size), Color.White, shader: _shader);
        }
    }
}