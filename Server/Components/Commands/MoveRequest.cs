using System.Numerics;
using MessagePack;
using Server.Attributes;

namespace Server.Components.Commands;

[RequestComponent]
public partial struct MoveRequest
{
    [Key(0)] public Vector2 Direction;
}