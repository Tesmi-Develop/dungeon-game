using Hypercube.Utilities.Dependencies;
using Server.Components.Events;
using Server.Utilities;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Events;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems.StateHandlers;

[EcsSystem]
public class AnimationStateHandlerSystem : BaseSystem
{
    [Dependency] private AnimatorSystem _animatorSystem = null!;
    
    public override void BeforeInitialize()
    {
        Subscribe<State, StateUpdated>((entity, ref state, ref _) =>
        {
            if (!HasComponent<AnimationStateMapping>(entity))
                return;
            
            ref var reference = ref GetComponent<AnimationStateMapping>(entity);
            
            if (!reference.Animations.TryGetValue(state.Current, out var clipName))
                return;
            
            _animatorSystem.Play(entity, clipName);
        });
    }
}