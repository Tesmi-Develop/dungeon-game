using Hypercube.Mathematics.Vectors;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components;

[SyncComponent]
public partial struct MovingDirection
{
    public Vector2 Direction;
}