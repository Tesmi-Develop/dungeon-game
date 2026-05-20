using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class PlayerTargetScannerSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<PlayerTargetTag, Target, NetworkTransform>();
    private readonly QueryMeta _playerQuery = new QueryMeta().WithAll<PlayerCharacter, NetworkTransform>();
    
    [Priority(EcsPriority.TargetScanner)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With((Entity e, ref Target target, ref NetworkTransform transform) =>
        {
            if (target.TargetAcquisitionRadius > target.TargetRetentionRadius)
            {
                Logger.Warning($"TargetAcquisitionRadius cannot be greater than TargetRetentionRadius, {e}");
                return;
            }
            
            if (target.TargetEntity.HasValue && (!EntityAlive(target.TargetEntity.Value) ||
                                                 !HasComponent<NetworkTransform>(target.TargetEntity.Value))
                )
            {
                target.TargetEntity = null;
                Logger.Trace("Lost target");
            }
            
            if (target.TargetEntity.HasValue)
            {
                var targetTransform = GetComponent<NetworkTransform>(target.TargetEntity.Value);
                if (ValidateDistance(targetTransform, transform, target.TargetRetentionRadius))
                    return;
                
                target.TargetEntity = null;
                Logger.Trace("Lost target");
            }
            
            foreach (var characterEntity in Query(_playerQuery))
            {
                var targetTransform = GetComponent<NetworkTransform>(characterEntity);
                if (!ValidateDistance(targetTransform, transform, target.TargetAcquisitionRadius))
                    continue;
                
                target.TargetEntity = characterEntity;
                Logger.Trace($"Found new target {characterEntity.Id}");
                break;
            }
        });
    }

    private bool ValidateDistance(NetworkTransform targetTransform, NetworkTransform myTransform, int targetRadius)
    {
        var distance = targetTransform.Position.Distance(myTransform.Position);
        return distance <= targetRadius;
    }
}