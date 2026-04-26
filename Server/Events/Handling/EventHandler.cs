using Arch.Core;

namespace Server.Events.Handling;

public delegate void EventHandler<TEvent>(ref TEvent args)
    where TEvent : IEvent;

public delegate void EventHandler<TComponent, TEvent>(Entity entity, ref TComponent component, ref TEvent args)
    where TEvent : IEvent;
