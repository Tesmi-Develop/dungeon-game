using Hypercube.Ecs.Events;

namespace Shared.Events;

public struct StageUpdated : IEvent
{
    public uint Current;
    public uint Previous;
}