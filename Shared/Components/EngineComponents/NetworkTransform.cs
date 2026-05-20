using System.Runtime.InteropServices;
using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;

namespace Shared.Components.EngineComponents;

[SyncComponent]
[StructLayout(LayoutKind.Sequential)]
public partial struct NetworkTransform : IComponent
{
    public Vector2 Position = Vector2.Zero;

    public NetworkTransform()
    {
    }
}