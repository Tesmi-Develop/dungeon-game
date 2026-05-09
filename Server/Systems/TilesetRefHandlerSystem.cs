using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Physics.Shapes;
using Server.Utilities;
using Shared.Components;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem]
public class TilesetRefHandlerSystem : BaseSystem
{
    private Query _query = null!;
    private readonly List<Entity> _entities = [];

    public override void Initialize()
    {
        _query = GetQuery().WithAll<TilesetRefComponent, HitboxDeclarationComponent>().WithNone<HitboxComponent>()
            .Build();
    }

    public override void GameUpdate(long tick, long _)
    {
        foreach (var entity in World.CollectEntities(_query, _entities))
        {
            ref var hitboxDeclaration = ref World.Get<HitboxDeclarationComponent>(entity);
            ref var tilesetRef = ref  World.Get<TilesetRefComponent>(entity);
            
            if (hitboxDeclaration.ShapeType == ShapeType.Polygon)
                World.AddCollision(entity, tilesetRef.Size, isStatic: true);
        }
    }
}