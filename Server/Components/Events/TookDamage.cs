using Hypercube.Ecs.Components;
using Hypercube.Ecs.Events;

namespace Server.Components.Events;

public struct TookDamage : IEvent
{
    public int Value;
}