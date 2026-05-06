using Client.Extensions;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs.Queries;
using Hypercube.Physics.Shapes;
using Shared.Components;

namespace Client.Systems;

public class TilesetRefHandlerSystem : EntitySystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<TilesetRefComponent, HitboxDeclarationComponent>().WithNone<HitboxComponent>()
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