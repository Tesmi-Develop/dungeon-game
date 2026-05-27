using Hypercube.Ecs.Components;
using Hypercube.Ecs.Events;

namespace Shared.Events;

public struct ComponentDirtyEvent<T> : IEvent where T : struct, IComponent
{
    public long Tick;
    public T Previous;

    public ComponentDirtyEvent(T previous)
    {
        Previous = previous;
    }
}