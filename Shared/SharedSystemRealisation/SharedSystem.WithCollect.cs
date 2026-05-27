using System.Runtime.CompilerServices;
using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Hypercube.Ecs.Entities;
using Hypercube.Ecs.Queries;
using Shared.Extensions;

namespace Shared.SharedSystemRealisation;

public partial class SharedSystem
{
    private List<Entity> _entities = [];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1>(QueryMeta meta, EntityRefAction<T1> action) where T1 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2>(QueryMeta meta, EntityRefAction<T1, T2> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2, T3>(QueryMeta meta, EntityRefAction<T1, T2, T3> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity), ref World.Get<T3>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2, T3, T4>(QueryMeta meta, EntityRefAction<T1, T2, T3, T4> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent 
        where T3 : struct, IComponent where T4 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity), 
                           ref World.Get<T3>(entity), ref World.Get<T4>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2, T3, T4, T5>(QueryMeta meta, EntityRefAction<T1, T2, T3, T4, T5> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent 
        where T3 : struct, IComponent where T4 : struct, IComponent where T5 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity), 
                           ref World.Get<T3>(entity), ref World.Get<T4>(entity), 
                           ref World.Get<T5>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2, T3, T4, T5, T6>(QueryMeta meta, EntityRefAction<T1, T2, T3, T4, T5, T6> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent 
        where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity), 
                           ref World.Get<T3>(entity), ref World.Get<T4>(entity), 
                           ref World.Get<T5>(entity), ref World.Get<T6>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2, T3, T4, T5, T6, T7>(QueryMeta meta, EntityRefAction<T1, T2, T3, T4, T5, T6, T7> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent 
        where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent 
        where T7 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity), 
                           ref World.Get<T3>(entity), ref World.Get<T4>(entity), 
                           ref World.Get<T5>(entity), ref World.Get<T6>(entity), 
                           ref World.Get<T7>(entity));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void With<T1, T2, T3, T4, T5, T6, T7, T8>(QueryMeta meta, EntityRefAction<T1, T2, T3, T4, T5, T6, T7, T8> action) 
        where T1 : struct, IComponent where T2 : struct, IComponent where T3 : struct, IComponent 
        where T4 : struct, IComponent where T5 : struct, IComponent where T6 : struct, IComponent 
        where T7 : struct, IComponent where T8 : struct, IComponent
    {
        foreach (var entity in World.CollectEntities(Query(meta), _entities))
            action(entity, ref World.Get<T1>(entity), ref World.Get<T2>(entity), 
                           ref World.Get<T3>(entity), ref World.Get<T4>(entity), 
                           ref World.Get<T5>(entity), ref World.Get<T6>(entity), 
                           ref World.Get<T7>(entity), ref World.Get<T8>(entity));
    }
}