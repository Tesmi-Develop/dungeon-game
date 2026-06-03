using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Object = DotTiled.Layers.Objects.Object;

namespace Shared.Components.MapComponents;

public struct TiledObjectComponent : IComponent
{
    public Object Object;
    public Vector2 Scale;
}