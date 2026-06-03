using DotTiled;
using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.ResourcesData;

namespace Shared.Components.EngineComponents;

public struct TilesetRefComponent : IComponent
{
    public Vector2 Size;
    public Tileset Ref;
}