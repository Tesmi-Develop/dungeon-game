using System.Diagnostics.CodeAnalysis;
using Hypercube.Core.Resources;
using Hypercube.Mathematics.Shapes;
using Hypercube.Utilities.Dependencies;
using Server.Utilities;
using Shared;
using Shared.ResourcesData;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem]
public class AnimationContainer : BaseSystem
{
    private const string ResourcesFolder = "Resources";
    [Dependency] private readonly IResourceManager _resourceManager = null!;
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
    
    public override void BeforeInitialize()
    {
        _animations = CollectAllJsonAnimations();
        
        foreach (var animationPath in _animations)
        {
            var clip = _resourceManager.Load<AnimationClip>(animationPath);
            var frameCounts = clip.Grid.Width * clip.Grid.Height;
            var totalTicks = (int)Math.Round(clip.Duration * Config.TickRate);
            
            clip.TicksPerFrame = Math.Max(1, totalTicks / frameCounts);
            clip.Frames = new Rect2[frameCounts];

            foreach (var eventData in clip.Events)
            {
                if (!clip.EventsByFrameIndex.TryGetValue(eventData.FrameIndex, out var events))
                {
                    events = [];
                    clip.EventsByFrameIndex.Add(eventData.FrameIndex, events);
                }

                events.Add(eventData);
            }
            
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