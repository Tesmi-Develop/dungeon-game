using Client.LifeCycles;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Shared.Components;
using Shared.Components.Commands;

namespace Client.Systems.Collisions;

public class CollisionWorldSystem : EntitySystem
{
    private const int CellSize = 128;
    
    // ChunkId -> List<Entity>
    private readonly Dictionary<Vector2i, List<Entity>> _grid = new();
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, HitboxComponent>().Build();
        /*World.SubscribeComponentRemoved<GridPresenceComponent>((in entity, ref presence) =>
        {
            UnregisterEntity(entity, ref presence);
        });*/
    }

    public override void AfterUpdate(FrameEventArgs args)
    {
        _query.With<NetworkTransform, HitboxComponent>((entity, ref trans, ref hitbox) =>
        {
            var currentGridIndex = WorldToGrid(trans.Position);
        
            if (currentGridIndex == hitbox.GridIndex)
                return;
        
            UpdateRegistration(entity, ref hitbox, currentGridIndex);
        });
    }

    private void UpdateRegistration(Entity entity, ref HitboxComponent hitbox, Vector2i currentGridIndex)
    {
        if (hitbox.GridIndex is { } prev)
        {
            if (!_grid.TryGetValue(prev, out var list)) 
                return;
            
            list.Remove(entity);
        }
        
        hitbox.GridIndex = currentGridIndex;

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
    
    public void UnregisterEntity(Entity entity, ref HitboxComponent hitbox)
    {
        if (hitbox.GridIndex is not { } prev) 
            return;
        
        if (_grid.TryGetValue(prev, out var list))
            list.Remove(entity);
    }

    public Vector2i WorldToGrid(Vector2 pos) => 
        new((int)(pos.X / CellSize), (int)(pos.Y / CellSize));
}