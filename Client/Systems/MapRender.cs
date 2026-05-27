using Client.Utilities;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Resources;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class MapRender : BaseSystem, IPatch
{
    [Dependency] private readonly MapHandler _mapHandler = null!;
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    public int Priority => 2;

    public override void Initialize()
    {
        _mapHandler.Compile(_resourceManager);
        var prototypeStorage = _resourceManager.Load<PrototypeStorage>("/prototypes.json");
        _mapHandler.Load(World, prototypeStorage, Vector2.Zero, Vector2.One / 2, new Vector2(2));
    }

    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        _mapHandler.Draw(renderer, Vector2.Zero, Vector2.One / 2, new Vector2(2));
    }
}