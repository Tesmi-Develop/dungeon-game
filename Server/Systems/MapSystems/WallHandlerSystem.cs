using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Physics.Shapes;
using Server.Utilities;
using Shared.Components.EngineComponents;
using Shared.Components.MapComponents;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.MapSystems;

[EcsSystem]
public class WallHandlerSystem : BaseSystem
{
    private readonly List<Entity> _entities = [];
    private readonly QueryMeta _meta = new QueryMeta().WithAll<Wall>().WithNone<CollisionComponent>();

    public override void GameUpdate(long tick, long predictTick)
    {
        foreach (var entity in World.CollectEntities(Query(_meta), _entities))
        {
            ref var wallDeclaration = ref World.Get<Wall>(entity);
            ref var tilesetRef = ref  World.Get<TilesetRefComponent>(entity);
            
            if (wallDeclaration.ShapeType == ShapeType.Polygon)
                World.AddCollision(entity, tilesetRef.Size, isStatic: true);
        }
    }
}