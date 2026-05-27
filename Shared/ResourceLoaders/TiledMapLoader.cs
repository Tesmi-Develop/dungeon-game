using System.Text.Json;
using Hypercube.Core.Graphics.Utilities.Extensions;
using Hypercube.Core.Resources;
using Hypercube.Core.Resources.FileSystems;
using Hypercube.Core.Resources.Loaders;
using Shared.ResourcesData;

namespace Shared.ResourceLoaders;

public class TiledMapLoader : ResourceLoader<TiledMap>
{
    private static JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public override string[] Extensions => ["tmj"];
    
    public override bool CanLoad(ResourcePath path, IFileSystem fileSystem)
    {
        return Extensions.Contains(path.Extension, StringComparer.OrdinalIgnoreCase);
    }

    public override TiledMap Load(ResourcePath path, IFileSystem fileSystem)
    {
        var file = fileSystem.OpenRead(path);
        var source = file.ReadToEnd();
        var map = JsonSerializer.Deserialize<TiledMap>(source, _options);
        return map ?? throw new Exception("Cloud not parse Tiled JSON.");
    }
}