using Client.Components;
using Client.Utilities;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class SnapshotCollectorSystem : BaseSystem
{
    [Dependency] private readonly GameClient _client = null!;
    private Query _query = null!;
    
    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, Interpolation>().Build();
    }

    public override void GameUpdate(long serverTick, long predictTick)
    {
        _query.With<NetworkTransform, Interpolation>((_, ref networkTransform, ref interpolation) =>
        {
            var pos = networkTransform.Position;
            
            if (interpolation.Snapshots.Count > 0 && serverTick <= interpolation.Snapshots.Last().Tick)
                return;
            
            if (pos == interpolation.LastPosition && interpolation.Snapshots.Count > 0)
            {
                return; 
            }

            interpolation.LastPosition = pos;
            interpolation.Snapshots.Enqueue((serverTick, pos));
            
            while (interpolation.Snapshots.Count > 20)
                interpolation.Snapshots.Dequeue();
        });
    }
}