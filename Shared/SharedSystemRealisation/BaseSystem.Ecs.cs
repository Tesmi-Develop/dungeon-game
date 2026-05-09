using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Hypercube.Ecs.Events;
using Hypercube.Ecs.Events.Handling;
using Hypercube.Ecs.Queries;

namespace Shared.SharedSystemRealisation;

public partial class SharedSystem
{
    protected Entity EntityCreate() =>
        World.Create();
    
    protected void EntityDestroy(Entity entity) =>
        World.Delete(entity);
    
    protected bool EntityAlive(Entity entity) =>
        World.Validate(entity);
    
    protected ref TComp GetComponent<TComp>(Entity entity) where TComp : struct, IComponent
        => ref World.Get<TComp>(entity);

    protected ref TComp AddComponent<TComp>(Entity entity) where TComp : struct, IComponent
        => ref World.Add<TComp>(entity);
    
    protected ref TComp AddComponent<TComp>(Entity entity, in TComp value) where TComp : struct, IComponent
        => ref World.Add(entity, value);

    protected bool HasComponent<TComp>(Entity entity) where TComp : struct, IComponent
        => World.Has<TComp>(entity);

    protected bool TryGetComponent<TComp>(Entity entity, out TComp component) where TComp : struct, IComponent
    {
        component = default;
        
        if (!World.Has<TComp>(entity))
            return false;
        
        component = World.Get<TComp>(entity);
        return true;
    }
    
    protected void RemoveComponent<TComp>(Entity entity) where TComp : struct, IComponent
        => World.Remove<TComp>(entity);

    protected Query Query(in QueryMeta meta)
        => World.Query(meta);
    
    protected QueryBuilder GetQuery()
        => new(World);
    
    protected Query CreateQuery(in QueryMeta meta)
        => World.Query(meta);

    protected void Subscribe<TComponent, TEvent>(Hypercube.Ecs.Events.Handling.EventHandler<TComponent, TEvent> handler, int priority = EventHandlerList.NoPriority)
        where TComponent : IComponent
        where TEvent : struct, IEvent
        => World.Events.Subscribe(handler, priority);

    protected void Unsubscribe<TComponent, TEvent>(Hypercube.Ecs.Events.Handling.EventHandler<TComponent, TEvent> handler)
        where TComponent : IComponent
        where TEvent : struct, IEvent
        => World.Events.Unsubscribe(handler);

    protected void Raise<TComponent, TEvent>(Entity entity, ref TComponent component, TEvent args)
        where TComponent : IComponent
        where TEvent : struct, IEvent
        => World.Events.Raise(entity, ref component, args);

    protected void Raise<TComponent, TEvent>(Entity entity, ref TComponent component, ref TEvent args)
        where TComponent : IComponent
        where TEvent : struct, IEvent
        => World.Events.Raise(entity, ref component, ref args);
        
    protected void Subscribe<TEvent>(Hypercube.Ecs.Events.Handling.EventHandler<TEvent> handler, int priority = EventHandlerList.NoPriority)
        where TEvent : struct, IEvent
        => World.Events.Subscribe(handler, priority);

    protected void Unsubscribe<TEvent>(Hypercube.Ecs.Events.Handling.EventHandler<TEvent> handler)
        where TEvent : struct, IEvent
        => World.Events.Unsubscribe(handler);
    
    protected void Raise<TEvent>(TEvent args)
        where TEvent : struct, IEvent
        => World.Events.Raise(ref args);
    
    protected void Raise<TEvent>(ref TEvent args)
        where TEvent : struct, IEvent
        => World.Events.Raise(ref args);
}