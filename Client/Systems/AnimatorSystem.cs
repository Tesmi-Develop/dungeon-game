using Client.Components.AnimationComponents;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.ResourcesData;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class AnimatorSystem : BaseSystem
{
    [Dependency] private readonly AnimationContainer _animationContainer = null!;
    [Dependency] private readonly EntrySystem _entrySystem = null!;
    private QueryMeta _query = new QueryMeta().WithAll<SpriteComponent, Animator>();

    public override void GameUpdate(long tick, long predictTick)
    {
        var currentTick = (int)tick;

        Query(_query).With((Entity entity, ref Animator animator, ref SpriteComponent sprite) =>
        {
            var clip = animator.CurrentClip;
            
            if (clip is null || !animator.IsPlaying)
                return;

            if (animator.IsPaused)
            {
                animator.PausedTicks++;
                return;
            }
            
            var elapsedTicks = currentTick - animator.StartTick - animator.PausedTicks;
            if (elapsedTicks < 0) 
                return;
            
            var frameIndex = elapsedTicks / clip.TicksPerFrame;
            var totalFrames = clip.Frames.Length;

            if (animator.IsLooping)
            {
                frameIndex %= totalFrames;
            }
            else if (frameIndex >= totalFrames)
            {
                frameIndex = totalFrames - 1;
                animator.IsPlaying = false;
            }
            
            sprite.Uv = clip.Frames[frameIndex];
            sprite.Texture = clip.Texture;
            
            var scaleX = (float)clip.FrameSize.Width / clip.TextureSize.X;
            var scaleY = (float)clip.FrameSize.Height / clip.TextureSize.Y;
            
            sprite.Scale = new Vector2(scaleX, scaleY) * animator.Scale;
        });
    }

    public void Play(Entity entity, string clipName)
    {
        Play(entity, clipName, null);
    }
    
    public void Play(Entity entity, string clipName, bool? loop)
    {
        if (!HasComponent<SpriteComponent>(entity))
            AddComponent<SpriteComponent>(entity);

        ref var animator = ref !HasComponent<Animator>(entity) ? ref AddComponent<Animator>(entity) : ref GetComponent<Animator>(entity);
        var clip = _animationContainer.GetClip(clipName);
        Play(ref animator, clip, loop);
    }
    
    public void Play(ref Animator animator, AnimationClip clip, bool? loop)
    {
        animator.CurrentClip = clip;
        animator.StartTick = (int)_entrySystem.CurrentTick;
        animator.PausedTicks = 0;
        animator.IsPlaying = true;
        animator.IsPaused = false;
        animator.IsLooping = loop ?? clip.Loop;
    }
    
    public void Stop(ref Animator animator)
    {
        animator.IsPlaying = false;
        animator.IsPaused = false;
        animator.PausedTicks = 0;
    }
    
    public void Pause(ref Animator animator)
    {
        animator.IsPaused = true;
    }
    
    public void Resume(ref Animator animator)
    {
        animator.IsPaused = false;
    }
}