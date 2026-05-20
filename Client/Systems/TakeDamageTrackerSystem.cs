using Client.Utilities;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Events;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class TakeDamageTrackerSystem : BaseSystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly TakeDamageEffectSystem _damageEffectSystem = null!;
    
    public override void Initialize()
    {
        Subscribe<Health, ComponentDirtyEvent<Health>>((entity, ref component, ref args) =>
        {
            if (!HasComponent<PlayerCharacter>(entity))
                return;
            
            ref var comp = ref GetComponent<PlayerCharacter>(entity);
            if (comp.ClientId != _gameClient.Id)
                return;
            
            if (args.Previous.Current <= component.Current)
                return;

            var delta = args.Previous.Current - component.Current;
            var intensity = delta / (float)component.Max;
            _damageEffectSystem.Invoke(intensity);
        });
    }
}