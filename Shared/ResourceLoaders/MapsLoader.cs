using System.Text.Json;
using Hypercube.Core.Graphics.Utilities.Extensions;
using Hypercube.Core.Resources;
using Hypercube.Core.Resources.FileSystems;
using Hypercube.Core.Resources.Loaders;
using Shared.Data;
using Shared.ResourcesData;

namespace Shared.ResourceLoaders;

public class MapsLoader : ResourceLoader<Maps>
{
    public override string[] Extensions => ["json"];
    
    public override bool CanLoad(ResourcePath path, IFileSystem fileSystem)
    {
        return Extensions.Contains(path.Extension, StringComparer.OrdinalIgnoreCase);
    }

    public override Maps Load(ResourcePath path, IFileSystem fileSystem)
    {
        var file = fileSystem.OpenRead(path);
        var source = file.ReadToEnd();
        return new Maps(JsonSerializer.Deserialize<Dictionary<uint, MapInfo>>(source) ?? []);
    }
}