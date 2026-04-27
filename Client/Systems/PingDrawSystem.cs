using System.Text;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Systems;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;

namespace Client.Systems;

public class PingDrawSystem : PatchEntitySystem
{
    [Dependency] private GameClient _gameClient = null!;
    [Dependency] private readonly IResourceManager _resource = null!;
    
    private Font _font = null!;

    public override void Initialize()
    {
        _font = _resource.Load<Font>("/fonts/OpenSans.ttf", [("size", 18)]);
    }

    public override void Draw(IRenderContext renderer, DrawPayload payload)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Ping: {_gameClient.Ping}");
        
        using (renderer.UseRenderState(payload.Window))
        {
            renderer.DrawText(sb.ToString(), _font, new Vector2(0, -16), Color.White);
        }
    }
}