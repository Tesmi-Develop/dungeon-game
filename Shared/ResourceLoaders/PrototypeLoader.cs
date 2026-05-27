using Hypercube.Core.Graphics.Utilities.Extensions;
using Hypercube.Core.Resources;
using Hypercube.Core.Resources.FileSystems;
using Hypercube.Core.Resources.Loaders;

namespace Shared.ResourceLoaders;

public class PrototypeLoader : ResourceLoader<PrototypeStorage>
{
    public override string[] Extensions => ["json"];
    
    public override bool CanLoad(ResourcePath path, IFileSystem fileSystem)
    {
        return Extensions.Contains(path.Extension, StringComparer.OrdinalIgnoreCase);
    }

    public override PrototypeStorage Load(ResourcePath path, IFileSystem fileSystem)
    {
        var file = fileSystem.OpenRead(path);
        var source = file.ReadToEnd();
        return PrototypeStorage.FromJson(source);
    }
}