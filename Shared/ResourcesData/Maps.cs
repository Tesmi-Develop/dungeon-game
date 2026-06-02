using Hypercube.Core.Resources;
using Hypercube.Core.Resources.Loaders;
using Shared.Data;

namespace Shared.ResourcesData;

public class Maps : Resource
{
    public readonly Dictionary<uint, MapRender> MapRenders = [];
    public readonly Dictionary<string, MapRender> MapRendersByName = [];
    private readonly Dictionary<uint, MapInfo> _maps;
    private readonly Dictionary<string, MapInfo> _mapInfos = [];

    public Maps(Dictionary<uint, MapInfo> maps)
    {
        _maps = maps;
    }

    public void Compile(IResourceManager resourceManager)
    {
        foreach (var (key, mapInfo) in _maps)
        {
            var render = new MapRender(mapInfo.Path);
            render.Compile(resourceManager);
            MapRenders[key] = render;
            MapRendersByName[mapInfo.Name] = render;
            _mapInfos[mapInfo.Name] = mapInfo;
        }
    }

    public MapRender GetMapRenderByStage(uint mapId)
    {
        return MapRenders[mapId];
    }

    public MapRender GetMapRenderByName(string mapName)
    {
        return MapRendersByName[mapName];
    }

    public MapInfo GetMapInfoByStage(uint mapId)
    {
        return _maps[mapId];
    }

    public MapInfo GetMapInfoByName(string mapName)
    {
        return _mapInfos[mapName];
    }
    
    public override void Dispose()
    {
        
    }
}