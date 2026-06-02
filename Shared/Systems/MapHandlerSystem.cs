using Hypercube.Core.Resources;
using Hypercube.Utilities.Dependencies;
using Shared.ResourcesData;
using Shared.SharedSystemRealisation;

namespace Shared.Systems;

public abstract class SharedMapHandlerSystem : SharedSystem
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    public Maps Maps { get; private set; }
    
    public override void Initialize()
    {
        Maps = _resourceManager.Load<Maps>("/maps.json");
        Maps.Compile(_resourceManager);
    }

    public MapRender GetMap(string name)
    {
        return Maps.GetMapRenderByName(name);
    }
}