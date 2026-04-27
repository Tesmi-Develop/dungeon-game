using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct Transform : IComponent
{
    public Vector2 Position = Vector2.Zero;

    public Transform()
    {
    }
}