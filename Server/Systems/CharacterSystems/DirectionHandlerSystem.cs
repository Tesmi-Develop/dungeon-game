using Hypercube.Ecs.Queries;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.CharacterSystems;

[EcsSystem]
public class DirectionHandlerSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<MovingDirection, NetworkTransform, Animator>().WithAll<ControlRotationByDirection>();

    [Priority(EcsPriority.BeforeApplyDirection)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<MovingDirection, NetworkTransform, Animator>((entity, ref movingDirection, ref transform, ref animator) =>
        {
            var prevScale = animator.Scale;
            
            animator.Scale = animator.Scale.WithX(movingDirection.Direction.X < 0 ? -1 : 1);
            
            if (animator.Scale == prevScale)
                return;
            
            NetworkHelper.MakeDirty<Animator>(World, entity);
        });
    }
}