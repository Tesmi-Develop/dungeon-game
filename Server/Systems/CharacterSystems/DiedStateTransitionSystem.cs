using Hypercube.Ecs.Queries;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.CharacterSystems;

[EcsSystem]
public class DiedStateTransitionSystem : BaseSystem
{
    private readonly QueryMeta _meta = new QueryMeta().WithAll<State, Health>();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        With<Health>(_meta, (entity, ref _) =>
        {
            if (World.IsAliveCharacter(entity) && HasComponent<Died>(entity))
            {
                World.SetState<Idle>(entity);
                return;
            }
            
            if (World.IsAliveCharacter(entity))
                return;
            
            World.SetState<Died>(entity);
        });
    }
}