using System.Diagnostics.CodeAnalysis;
using Hypercube.Core.Ecs;
using Hypercube.Core.Graphics.Rendering.Api;
using Hypercube.Core.Graphics.Rendering.Manager;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Resources.Preloading;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared;
using Shared.ResourcesData;

namespace Client.InternalSystems;

public class AnimationContainer
{
    private const string ResourcesFolder = "Resources";
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    [Dependency] private readonly PreloadContext _preloadContext = null!;
    [Dependency] private readonly IRenderManager _renderManager = null!;
    private ResourcePath[] _animations = null!;
    private Dictionary<string, AnimationClip> _clips = [];

    public AnimationClip GetClip(string clipName)
    {
        return _clips[clipName];
    }

    public bool TryGetClip(string clipName, [MaybeNullWhen(false)] out AnimationClip clip)
    {
        return _clips.TryGetValue(clipName, out clip);
    }
    
    public void Initialize()
    {
        _animations = CollectAllJsonAnimations();
        _preloadContext.Add<AnimationClip>(_animations);
    }

    public void Start()
    {
        foreach (var animationPath in _animations)
        {
            var clip = _resourceManager.Load<AnimationClip>(animationPath);
            var imagePath = animationPath + new ResourcePath(clip.Image);
            var texture = _resourceManager.Load<Texture>(imagePath);
            
            clip.Texture = texture;
            texture.GpuBind(_renderManager.Api);

            var texSize = new Vector2i(clip.FrameSize.Width * clip.Grid.Width, clip.FrameSize.Height * clip.Grid.Height);
            var tileSize = new Vector2(clip.FrameSize.Width, clip.FrameSize.Height);
            var frameCounts = clip.Grid.Width * clip.Grid.Height;
            var frames = new Rect2[frameCounts];
            
            var totalTicks = (int)Math.Round(clip.Duration * Config.TickRate);
            clip.TicksPerFrame = Math.Max(1, totalTicks / frameCounts);
            
            for (var i = 0; i < frameCounts; i++)
            {
                var column = i % clip.Grid.Width;
                var row = i / clip.Grid.Width;
            
                var uvTopLeft = new Vector2(column * tileSize.X, (row + 1) * tileSize.Y) / texSize;
                var uvBottomRight = new Vector2((column + 1) * tileSize.X, row * tileSize.Y) / texSize;
                frames[i] =  new Rect2(uvTopLeft, uvBottomRight);
            }
            
            foreach (var eventData in clip.Events)
            {
                if (!clip.EventsByFrameIndex.TryGetValue(eventData.FrameIndex, out var events))
                {
                    events = [];
                    clip.EventsByFrameIndex.Add(eventData.FrameIndex, events);
                }

                events.Add(eventData);
            }

            clip.Frames = frames;
            clip.TextureSize = new Vector2i(clip.FrameSize.Width * clip.Grid.Width, clip.FrameSize.Height * clip.Grid.Height);
            _clips.Add(clip.Name, clip);
        }
    }

    private ResourcePath[] CollectAllJsonAnimations()
    {
        return Directory.EnumerateFiles("resources", "Info.json", SearchOption.AllDirectories)
            .Select(Path.GetDirectoryName)
            .Where(path => path is not null)
            .Select(path => new ResourcePath(path!.Remove(0, ResourcesFolder.Length)))
            .ToArray();
    }
}