using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.CharacterSystems;

[EcsSystem]
public class EndAttackStateTransitionSystem : BaseSystem
{
    private readonly QueryMeta _meta = new QueryMeta().WithAll<Animator, Attacking>();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        With(_meta, (Entity entity, ref Animator animator) =>
        {
            if (animator.IsPlaying)
                return;

            World.SetState<Idle>(entity);
        });
    }
}