using Arch.Core;
using Shared.Data;

namespace Server.Events;

public interface IEventBus
{
    void Subscribe<TComponent, TEvent>(Handling.EventHandler<TComponent, TEvent> handler, EventBusPriority priority = EventBusPriority.NoPriority)
        where TEvent : struct, IEvent;

    void Unsubscribe<TComponent, TEvent>(Handling.EventHandler<TComponent, TEvent> handler)
        where TEvent : struct, IEvent;
    
    void Subscribe<TEvent>(Handling.EventHandler<TEvent> handler, EventBusPriority priority = EventBusPriority.NoPriority)
        where TEvent : struct, IEvent;

    void Unsubscribe<TEvent>(Handling.EventHandler<TEvent> handler)
        where TEvent : struct, IEvent;

    void Raise<TComponent, TEvent>(Entity entity, TEvent args)
        where TEvent : struct, IEvent;
    
    void Raise<TComponent, TEvent>(Entity entity, ref TEvent args)
        where TEvent : struct, IEvent;
    
    void Raise<TEvent>(TEvent args)
        where TEvent : struct, IEvent;

    void Raise(IEvent args);
    
    void Raise<TEvent>(ref TEvent args)
        where TEvent : struct, IEvent;
}
