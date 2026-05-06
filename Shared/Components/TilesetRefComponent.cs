using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.ResourcesData;

namespace Shared.Components;

public struct TilesetRefComponent : IComponent
{
    public Vector2 Size;
    public TiledTileset Ref;
}