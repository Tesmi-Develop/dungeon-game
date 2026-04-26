using System.Numerics;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct Transform
{
    public Vector2 Position = Vector2.Zero;

    public Transform()
    {
    }
}