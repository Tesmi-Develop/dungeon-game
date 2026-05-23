using Hypercube.Mathematics.Vectors;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct MovingDirection
{
    public Vector2 Direction;
}