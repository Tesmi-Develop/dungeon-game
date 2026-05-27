using Hypercube.Ecs;
using Hypercube.Ecs.Lifetime;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;
using Shared.Components;
using Shared.SharedSystemRealisation;
using CollisionComponent = Shared.Components.EngineComponents.CollisionComponent;
using NetworkTransform = Shared.Components.EngineComponents.NetworkTransform;

namespace Shared.Systems.Collisions;

[EcsSystem]
public class CollisionWorldSystem : SharedSystem
{
    private const int CellSize = 128;
    
    // ChunkId -> List<Entity>
    private readonly Dictionary<Vector2i, List<Entity>> _grid = new();
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, CollisionComponent>().Build();
        
        Subscribe((Entity entity, ref CollisionComponent collision, ref RemovedEvent args) =>
        {
            UnregisterEntity(entity, ref collision);
        });
    }

    [Priority(EcsPriority.UpdateCollisionWorld)]
    public override void GameUpdate(long tick, long _)
    {
        _query.With((Entity entity, ref NetworkTransform trans, ref CollisionComponent collision, ref CollisionComponent presence) =>
        {
            var currentGridIndex = WorldToGrid(trans.Position);
            
            if (currentGridIndex == presence.GridIndex)
                return;
        
            UpdateRegistration(entity, ref presence, currentGridIndex);
        });
    }

    private void UpdateRegistration(Entity entity, ref CollisionComponent presence, Vector2i currentGridIndex)
    {
        if (presence.GridIndex is { } prev)
        {
            if (!_grid.TryGetValue(prev, out var list)) 
                return;
            
            list.Remove(entity);
        }
        
        presence.GridIndex = currentGridIndex;

        {
            if (_grid.TryGetValue(currentGridIndex, out var list))
            {
                list.Add(entity);
                return;
            }
        }
        
        _grid[currentGridIndex] = [entity];
    }

    public void GetNearby(Vector2i gridId, List<Entity> result)
    {
        var rect2 = new Rect2i(gridId.X - 1, gridId.Y + 1, gridId.X + 1, gridId.Y - 1);

        for (var x = rect2.Left; x < rect2.Right + 1; x++)
        {
            for (var y = rect2.Bottom; y < rect2.Top + 1; y++)
            {
                if (!_grid.TryGetValue((x, y), out var list)) 
                    continue;

                result.AddRange(list);
            }
        }
    }
    
    public void UnregisterEntity(Entity entity, ref CollisionComponent presence)
    {
        if (presence.GridIndex is not { } prev) 
            return;
        
        if (_grid.TryGetValue(prev, out var list))
            list.Remove(entity);
    }

    public Vector2i WorldToGrid(Vector2 pos) => 
        new((int)(pos.X / CellSize), (int)(pos.Y / CellSize));
}