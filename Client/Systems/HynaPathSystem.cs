using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Systems;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using StbTrueTypeSharp;

namespace Client.Systems;

public class HynaPathSystem : PatchEntitySystem
{
    private Query _query;
    
    public override void Initialize()
    {
        base.Initialize();

        _query = GetQuery().WithAll<SpriteComponent, TransformComponent>().Build();
    }

    public override void Draw(IRenderContext renderer, DrawPayload payload)
    {
        Entity past = Entity.Invalid;
        
        _query.With<TransformComponent, SpriteComponent>((entity, ref transform, ref sprite) =>
        {
            /*sprite.Color = new Color((int)transform.LocalPosition.X * 1000000 + ((int)transform.LocalPosition.Y * 17));
            sprite.Color = sprite.Color.WithA(1);*/
            
            if (past != Entity.Invalid)
                renderer.DrawLine(transform.LocalPosition.Xy, GetComponent<TransformComponent>(past).LocalPosition.Xy, Color.Coral, 5);
            past = entity;
        });
    }
}