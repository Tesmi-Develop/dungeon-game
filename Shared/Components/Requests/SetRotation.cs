using MessagePack;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components.Requests;

[RequestComponent]
public partial struct SetRotation
{
    [Key(0)] public int Sign;
}