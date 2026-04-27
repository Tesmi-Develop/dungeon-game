using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;

namespace Client.Components;

public struct InterpolationComponent : IComponent
{
    public Queue<(float Time, Vector2 Position)> Snapshots = new();
    public float ClientInterpolationTime = 0; // Наше внутреннее "время прошлого"
    public Vector2 LastPosition = Vector2.Zero;

    public InterpolationComponent()
    {
    }
}