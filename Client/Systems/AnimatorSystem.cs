using Client.InternalSystems;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.ResourcesData;
using Shared.SharedSystemRealisation;
using Shared.Systems;

namespace Client.Systems;

[EcsSystem]
public class AnimatorSystem : SharedAnimatorSystem
{
    [Dependency] private readonly AnimationContainer _animationContainer = null!;
    [Dependency] private readonly EntrySystem _entrySystem = null!;
    private readonly QueryMeta _query = new QueryMeta().WithAll<SpriteComponent, Animator>();

    protected override long GetCurrentTick()
    {
        return _entrySystem.CurrentTick;
    }

    protected override AnimationClip GetAnimationClip(string clipName)
    {
        return _animationContainer.GetClip(clipName);
    }

    protected override void OnAnimationFrameIndexUpdate(Entity entity, ref Animator animator, int frameIndex)
    {
        if (!HasComponent<SpriteComponent>(entity))
            AddComponent<SpriteComponent>(entity);
        
        ref var sprite = ref GetComponent<SpriteComponent>(entity);
        var clip = animator.CurrentClip!;
        
        sprite.Uv = clip.Frames[frameIndex];
        sprite.Texture = clip.Texture;
            
        var scaleX = (float)clip.FrameSize.Width / clip.TextureSize.X;
        var scaleY = (float)clip.FrameSize.Height / clip.TextureSize.Y;
            
        sprite.Scale = new Vector2(scaleX, scaleY) * animator.Scale;
    }

    protected override void OnAnimatorUpdate(Entity entity, ref Animator animator)
    {
        // Nothing
    }
}