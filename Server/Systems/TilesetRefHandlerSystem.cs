using Arch.Core;
using Hypercube.Physics.Shapes;
using Server.Extensions;
using Shared.Components;

namespace Server.Systems;

[EcsSystem]
public class TilesetRefHandlerSystem : BaseSystem
{
    private QueryDescription _query = new QueryDescription().WithAll<TilesetRefComponent, HitboxDeclarationComponent>().WithNone<HitboxComponent>();

    public override void Initialize()
    {
        
    }

    public override void Update(long tick)
    {
        world.Query<TilesetRefComponent, HitboxDeclarationComponent>(in _query, (entity, ref tilesetRef,
            ref hitboxDeclaration) =>
        {
            if (hitboxDeclaration.ShapeType == ShapeType.Polygon)
                world.AddCollision(entity, tilesetRef.Size, isStatic: true);
        });
    }
}