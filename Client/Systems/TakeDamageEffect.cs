using GTweens.Tweens;
using Hypercube.Ecs.Components;

namespace Client.Systems;

public struct TakeDamageEffect : IComponent
{
    public GTween? Tween;
    public float Intensity;
    public float TargetIntensity;
}