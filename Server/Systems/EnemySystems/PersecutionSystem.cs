using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.EngineComponents;
using Shared.Components.States;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class PersecutionSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Speed, Target, NetworkTransform, Moving, MovingDirection>();

    [Priority(EcsPriority.AfterTargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With((Entity entity, ref NetworkTransform transform, ref Target target, ref Speed speed, ref MovingDirection movingDirection) =>
        {
            movingDirection.Direction = Vector2.Zero;
            
            if (!target.TargetEntity.HasValue)
                return;
            
            ref var targetTransform = ref GetComponent<NetworkTransform>(target.TargetEntity.Value);
            var delta = targetTransform.Position - transform.Position;
            
            if (delta.Length < 0.001)
                return;
            
            var direction = delta.Normalized;
            movingDirection.Direction = direction;
        });
    }
}