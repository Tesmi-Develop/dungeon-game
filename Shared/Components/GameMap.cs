using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct GameMap : IComponent
{
    public string MapName = string.Empty;
    public Vector2 Position = default;
    public Vector2 Anchor = default;
    public Vector2 Scale = default;

    public GameMap()
    {
    }
}