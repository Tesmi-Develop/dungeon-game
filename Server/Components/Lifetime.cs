using Hypercube.Ecs.Components;

namespace Server.Components;

public struct Lifetime : IComponent
{
    public int RemainingTicks;
}