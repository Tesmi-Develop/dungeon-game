using MessagePack;
using Shared.Attributes;

namespace Shared.Components.Requests;

[RequestComponent]
public partial struct SetRotation
{
    [Key(0)] public int Sign;
}