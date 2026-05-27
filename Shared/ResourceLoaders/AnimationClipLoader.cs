using System.Text.Json;
using Hypercube.Core.Graphics.Utilities.Extensions;
using Hypercube.Core.Resources;
using Hypercube.Core.Resources.FileSystems;
using Hypercube.Core.Resources.Loaders;
using Shared.ResourcesData;

namespace Shared.ResourceLoaders;

public class AnimationClipLoader : ResourceLoader<AnimationClip>
{
    private static JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    public override string[] Extensions => ["json"];
    
    public override bool CanLoad(ResourcePath path, IFileSystem fileSystem)
    {
        if (path.Extension == string.Empty)
            return fileSystem.Exists(path + "Info.json");
        
        return Extensions.Contains(path.Extension, StringComparer.OrdinalIgnoreCase);
    }

    public override AnimationClip Load(ResourcePath path, IFileSystem fileSystem)
    {
        if (path.Extension == string.Empty)
            path += path + "Info.json";
        
        var file = fileSystem.OpenRead(path);
        var source = file.ReadToEnd();
        var data = JsonSerializer.Deserialize<AnimationClip>(source, _options);
        return data ?? throw new NullReferenceException();
    }
}