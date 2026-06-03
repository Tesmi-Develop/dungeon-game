using DotTiled.Layers.Objects;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Server.Utilities;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Components.MapComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.MapSystems;

[EcsSystem]
public class LightSourceHandlerSystem : BaseSystem
{
    private readonly QueryMeta _meta = new QueryMeta().WithAll<SourceLight, TiledObjectComponent>().WithNone<Light>();

    public override void GameUpdate(long tick, long predictTick)
    {
        With<SourceLight, TiledObjectComponent>(_meta, (entity, ref source, ref objectRef) =>
        {
            if (objectRef.Object is EllipseObject ellipse)
                AddComponent(entity, new Light
                {
                    Radius = ellipse.Width * objectRef.Scale.X, 
                    Intensity = source.Intensity, 
                    Falloff = source.Falloff, 
                    LightType = LightType.Circle
                });
            
            if (objectRef.Object is RectangleObject rectangleObject)
                AddComponent(entity, new Light
                {
                    Size = new Vector2(rectangleObject.Width, rectangleObject.Height) * objectRef.Scale,
                    Intensity = source.Intensity, 
                    Falloff = source.Falloff, 
                    LightType = LightType.Rectangle
                });
        });
    }
}