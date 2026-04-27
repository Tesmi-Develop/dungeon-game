using Hypercube.Mathematics.Vectors;
using MessagePack;
using Shared.Attributes;

namespace Shared.Components.Commands;

[RequestComponent]
public partial struct MoveRequest
{
    [Key(0)] public Vector2 Direction;
}