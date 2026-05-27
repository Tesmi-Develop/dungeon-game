using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Shared.Components.Enemies;
using Shared.Events;

namespace Shared.Extensions;

public static class WorldStateExtensions
{
    extension(World world)
    {
        public T SetState<T>(Entity entity) where T : struct, IComponent
        {
            if (world.Has<T>(entity))
                return world.Get<T>(entity);

            if (!world.Has<State>(entity))
                world.Add(entity, new State { Current = typeof(T) });
            else
            {
                ref var state = ref world.Get<State>(entity);
                var prev = state.Current;
                state.Current = typeof(T);
                state.Prev = prev;
                world.Remove(entity, prev);
            }

            ref var comp = ref world.Add<T>(entity);
            ref var state1 = ref world.Get<State>(entity);
            world.Events.Raise(entity, ref state1, new StateUpdated());
            return comp;
        }
        
        public T SetState<T>(Entity entity, T data) where T : struct, IComponent
        {
            if (world.Has<T>(entity))
                return world.Get<T>(entity);

            if (!world.Has<State>(entity))
                world.Add(entity, new State { Current = typeof(T) });
            else
            {
                ref var state = ref world.Get<State>(entity);
                var prev = state.Current;
                state.Current = typeof(T);
                state.Prev = prev;
                world.Remove(entity, prev);
            }

            ref var comp = ref world.Add<T>(entity, data);
            ref var state1 = ref world.Get<State>(entity);
            world.Events.Raise(entity, ref state1, new StateUpdated());
            return comp;
        }
    }
}