using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs.Queries;
using Hypercube.Physics.Shapes;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class TilesetRefHandlerSystem : BaseSystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<TilesetRefComponent, HitboxDeclarationComponent>().WithNone<CollisionComponent>()
            .Build();
    }

    public override void Update(FrameEventArgs tick)
    {
        _query.With<TilesetRefComponent, HitboxDeclarationComponent>((entity, ref tilesetRef,
            ref hitboxDeclaration) =>
        {
            if (hitboxDeclaration.ShapeType == ShapeType.Polygon)
                World.AddCollision(entity, tilesetRef.Size, isStatic: true);
        });
    }
}