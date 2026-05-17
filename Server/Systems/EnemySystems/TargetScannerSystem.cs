using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Helpers;
using Shared.Components.Enemies;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class TargetScannerSystem : SharedSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Target>();

    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With((Entity entity, ref Target target) =>
        {
            if (!target.TargetEntity.HasValue) 
                return;
            
            if (target.TargetEntity.Value!.GetFullMask() ==  target.EntityMask)
                return;
            
            target.EntityMask = target.TargetEntity.Value!.GetFullMask();
            NetworkHelper.MakeDirty<Target>(World, entity);
        });
    }
}