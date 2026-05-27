using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;

namespace Shared.Components.States;

public struct Attacking : IComponent
{
    public Vector2 TargetPosition;
}