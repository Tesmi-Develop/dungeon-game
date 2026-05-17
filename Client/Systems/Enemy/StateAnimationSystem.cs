using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.Components.Enemies;

namespace Client.Systems.Enemy;

public class StateAnimationSystem : BaseSystem
{
    [Dependency] private readonly AnimationContainer _animationContainer = null!;
    [Dependency] private readonly AnimatorSystem _animator = null!;
    private QueryMeta _meta = new QueryMeta().WithAll<State, SpriteReference>();
    
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_meta).With((Entity entity, ref State state, ref SpriteReference reference) =>
        {
            if (state.StateType == state.PrevStateType)
                return;
            
            if (!reference.Animations.TryGetValue(state.StateType, out var animationName))
                return;
            
            _animator.Play(entity, animationName);
        });
    }
}