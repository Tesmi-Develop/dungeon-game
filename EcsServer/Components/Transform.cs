using System.Numerics;
using EcsServer.Attributes;

namespace EcsServer.Components;

[SyncComponent]
public partial struct Transform
{
    public Vector2 Position = Vector2.Zero;

    public Transform()
    {
    }
}