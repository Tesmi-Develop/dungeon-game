using Hypercube.Utilities.Dependencies;
using Server.Components.Events;
using Server.Utilities;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems.AnimationHandlers;

[EcsSystem]
public class DefaultStateHandlerSystem : BaseSystem
{
    [Dependency] private AnimatorSystem _animatorSystem = null!;
    
    public override void Initialize()
    {
        Subscribe<State, StateUpdated>((entity, ref state, ref args) =>
        {
            if (!HasComponent<SpriteReference>(entity))
                return;
            
            ref var reference = ref GetComponent<SpriteReference>(entity);
            string clipName;

            switch (state.StateType)
            {
                case StateType.Idle:
                    if (reference.Animations.TryGetValue(state.StateType, out clipName))
                        _animatorSystem.Play(entity, clipName);
                    break;
                
                case StateType.Moving:
                    if (reference.Animations.TryGetValue(state.StateType, out clipName))
                        _animatorSystem.Play(entity, clipName);
                    break;
                case StateType.Attacking:
                    if (reference.Animations.TryGetValue(state.StateType, out clipName))
                        _animatorSystem.Play(entity, clipName);
                    break;
            }
        });
    }
}