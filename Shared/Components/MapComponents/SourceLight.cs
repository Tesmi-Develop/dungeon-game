using Hypercube.Ecs.Components;

namespace Shared.Components.MapComponents;

public struct SourceLight : IComponent
{
    public float Intensity = 1f;
    public float Falloff = 0.3f;

    public SourceLight()
    {
    }
}