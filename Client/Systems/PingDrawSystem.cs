using System.Text;
using Client.Utilities;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class PingDrawSystem : BaseSystem, IPatch
{
    [Dependency] private GameClient _gameClient = null!;
    [Dependency] private readonly IResourceManager _resource = null!;
    public int Priority => 0;
    
    private Font _font = null!;

    public override void Initialize()
    {
        _font = _resource.Load<Font>("/fonts/OpenSans.ttf", [("size", 18)]);
    }

    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Ping: {_gameClient.Ping}");
        
        using (renderer.UseRenderState(payload.Window))
        {
            renderer.DrawText(sb.ToString(), _font, new Vector2(0, -16), Color.White);
        }
    }
}