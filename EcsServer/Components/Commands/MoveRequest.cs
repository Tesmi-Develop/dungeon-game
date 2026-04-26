using System.Numerics;
using EcsServer.Attributes;
using MessagePack;

namespace EcsServer.Components.Commands;

[RequestComponent]
public partial struct MoveRequest
{
    [Key(0)] public Vector2 Direction;
}