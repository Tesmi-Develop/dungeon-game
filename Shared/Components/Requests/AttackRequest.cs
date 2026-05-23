using Hypercube.Mathematics.Vectors;
using MessagePack;
using Shared.Attributes;

namespace Shared.Components.Requests;

[RequestComponent]
public partial struct AttackRequest
{
    [Key(0)] public Vector2 Direction;
}