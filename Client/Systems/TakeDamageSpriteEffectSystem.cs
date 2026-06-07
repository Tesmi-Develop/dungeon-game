using Client.Data;
using Client.Utilities;
using GTweens.Builders;
using GTweens.Easings;
using GTweens.Extensions;
using GTweens.Tweens;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Rendering.Manager;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem, Scene(SceneType.Game)]
public class TakeDamageSpriteEffectSystem : BaseSystem
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    [Dependency] private readonly IRenderContext _renderContext = null!;
    private readonly QueryMeta _meta = new QueryMeta().WithAll<TakeDamageEffect>();
    private Shader _shader = null!;

    public override void Initialize()
    {
        _shader = _resourceManager.Load<Shader>("/shaders/overlay.shd");
    }

    private float GetEffectIntensity(Entity entity)
    {
        if (!HasComponent<TakeDamageEffect>(entity))
            return 0;
        
        return GetComponent<TakeDamageEffect>(entity).Intensity;
    }
    
    private void SetEffectIntensity(Entity entity, float value)
    {
        if (!HasComponent<TakeDamageEffect>(entity))
            return;
        
        GetComponent<TakeDamageEffect>(entity).Intensity = value;
    }
    
    private float GetEffectTargetIntensity(Entity entity)
    {
        if (!HasComponent<TakeDamageEffect>(entity))
            return 0;
        
        return GetComponent<TakeDamageEffect>(entity).TargetIntensity;
    }

    private void PrepareTween(Entity entity, ref TakeDamageEffect effect)
    {
        effect.Tween = GTweenSequenceBuilder
            .New()
            .Append(GTweenExtensions.Tween(
                () => GetEffectIntensity(entity),
                x => SetEffectIntensity(entity, x),
                () => GetEffectTargetIntensity(entity),
                0.15f,
                ValidationExtensions.AlwaysValid
            ).SetEasing(Easing.OutExpo))
            .Append(GTweenExtensions.Tween(
                () => GetEffectTargetIntensity(entity),
                x => SetEffectIntensity(entity, x),
                0,
                1f,
                ValidationExtensions.AlwaysValid
            ).SetEasing(Easing.OutSine))
            .Build();

        effect.Tween.OnCompleteOrKill(() =>
        {
            RemoveComponent<TakeDamageEffect>(entity);
            
            if (!HasComponent<SpriteComponent>(entity))
                return;

            GetComponent<SpriteComponent>(entity).Shader = null;
        });
        effect.Tween.Start();
    }
    
    public void Invoke(float intensity, Entity entity)
    {
        intensity = Math.Clamp(intensity, 0, 1);
        
        if (HasComponent<TakeDamageEffect>(entity))
        {
            ref var effect = ref GetComponent<TakeDamageEffect>(entity);
            effect.TargetIntensity = intensity;
            PrepareTween(entity, ref effect);
            return;
        }
        
        if (!HasComponent<SpriteComponent>(entity))
            return;
        
        ref var spriteComponent = ref GetComponent<SpriteComponent>(entity);
        spriteComponent.Shader = _shader;
        
        ref var addedEffect = ref AddComponent<TakeDamageEffect>(entity);
        addedEffect.TargetIntensity = intensity;
        PrepareTween(entity, ref addedEffect);
    }

    public override void Update(FrameEventArgs args)
    {
        Query(_meta).With<TakeDamageEffect>((_, ref effect) =>
        {
            effect.Tween?.Tick((float)args.Delta.TotalSeconds);
            
            _renderContext.BindShader(_shader);
            _shader.SetUniform("intensity", effect.Intensity);
            _shader.SetUniform("overlayColor", Color.Red);
            _renderContext.ClearShader();
        });
    }
}