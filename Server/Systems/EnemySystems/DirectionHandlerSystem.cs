using Hypercube.Ecs.Queries;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class DirectionHandlerSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Target, NetworkTransform, Animator>();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<Target, NetworkTransform, Animator>((entity, ref target, ref transform, ref animator) =>
        {
            if (!target.TargetEntity.HasValue)
                return;
            
            var targetTransform = GetComponent<NetworkTransform>(target.TargetEntity.Value);
            var direction = (targetTransform.Position - transform.Position).Normalized;
            var prevScale = animator.Scale;
            
            animator.Scale = animator.Scale.WithX(direction.X < 0 ? -1 : 1);
            
            if (animator.Scale == prevScale)
                return;
            
            NetworkHelper.MakeDirty<Animator>(World, entity);
        });
    }
}