using System.Collections.Concurrent;
using Arch.Core;
using EcsServer.Data;
using EcsServer.Events.Handling;

namespace EcsServer.Events;

public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, EventHandlerList> _broadcastHandlers = new();
    private readonly ConcurrentDictionary<EventSignature, EventHandlerList> _handlers = new();

    #region Broadcast
    
    public void Subscribe<TEvent>(Handling.EventHandler<TEvent> handler, EventBusPriority priority = EventBusPriority.NoPriority)
        where TEvent : struct, IEvent
    {
        var signature = typeof(TEvent);
        GetBroadcastHandlers(signature).Add(handler, priority);
    }
    

    public void Unsubscribe<TEvent>(Handling.EventHandler<TEvent> handler)
        where TEvent : struct, IEvent
    {
        var signature = typeof(TEvent);
        if (!_broadcastHandlers.TryGetValue(signature, out var list))
            return;

        list.Remove(handler);
    }
    
    public void Raise<TEvent>(TEvent args)
        where TEvent : struct, IEvent
    {
        Raise(ref args);
    }

    public void Raise<TEvent>(ref TEvent args)
        where TEvent : struct, IEvent
    {
        var signature = typeof(TEvent);
        if (!_broadcastHandlers.TryGetValue(signature, out var list))
            return;
        
        list.Invoke(ref args);
    }

    #endregion
    
    public void Subscribe<TComponent, TEvent>(Handling.EventHandler<TComponent, TEvent> handler, EventBusPriority priority = EventBusPriority.NoPriority)
        where TEvent : struct, IEvent
    {
        var signature = new EventSignature(Component<TComponent>.ComponentType, typeof(TEvent));
        GetHandlers(signature).Add(handler, priority);
    }

    public void Unsubscribe<TComponent, TEvent>(Handling.EventHandler<TComponent, TEvent> handler)
        where TEvent : struct, IEvent
    {
        var signature = new EventSignature(Component<TComponent>.ComponentType, typeof(TEvent));
        if (!_handlers.TryGetValue(signature, out var list))
            return;

        list.Remove(handler);
    }

    public void Raise<TComponent, TEvent>(Entity entity, TEvent args)
        where TEvent : struct, IEvent 
        => Raise<TComponent, TEvent>(entity, ref args);

    public void Raise<TComponent, TEvent>(Entity entity, ref TEvent args)
        where TEvent : struct, IEvent
    {
        var signature = new EventSignature(Component<TComponent>.ComponentType, typeof(TEvent));
        if (!_handlers.TryGetValue(signature, out var list))
            return;

        var world = World.Worlds[entity.WorldId];
        ref var component = ref world.Get<TComponent>(entity);
        list.Invoke(entity, ref component, ref args);
    }

    private EventHandlerList GetHandlers(in EventSignature signature)
        => _handlers.GetOrAdd(signature, static _ => new EventHandlerList());
    
    private EventHandlerList GetBroadcastHandlers(in Type eventType)
        => _broadcastHandlers.GetOrAdd(eventType, static _ => new EventHandlerList());
}