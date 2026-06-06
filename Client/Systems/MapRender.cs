using Client.Data;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Resources;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.SharedSystemRealisation;
using Shared.Systems;

namespace Client.Systems;

[EcsSystem, Scene(SceneType.Game)]
public class MapRender : SharedMapHandlerSystem, IPatch
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    private readonly QueryMeta _meta = new QueryMeta().WithAll<GameMap>();
    public int Priority => 2;

    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        Query(_meta).With<GameMap>((entity, ref info) =>
        {
            if (info.MapName == string.Empty)
                return;

            var render = GetMap(info.MapName);
            render.Draw(renderer, payload.Camera, info.Position, info.Anchor, info.Scale);
        });
    }
}