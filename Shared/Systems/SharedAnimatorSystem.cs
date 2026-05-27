using System.Diagnostics.CodeAnalysis;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Shared.Attributes;
using Shared.Components;
using Shared.ResourcesData;
using Shared.SharedSystemRealisation;

namespace Shared.Systems;

public abstract class SharedAnimatorSystem : SharedSystem
{
    private readonly QueryMeta _query = new QueryMeta().WithAll<Animator>();

    protected abstract long GetCurrentTick();
    protected abstract AnimationClip GetAnimationClip(string clipName);
    protected abstract void OnAnimationFrameIndexUpdate(Entity entity, ref Animator animator, int frameIndex);
    protected abstract void OnAnimatorUpdate(Entity entity, ref Animator animator);
    
    [Priority(EcsPriority.High)]
    public override void BeforeGameUpdate(long tick, long predictTick)
    {
        var currentTick = (int)tick;

        Query(_query).With((Entity entity, ref Animator animator) =>
        {
            var clip = animator.CurrentClip;
            
            if (clip is null || !animator.IsPlaying)
                return;

            if (animator.IsPaused)
            {
                animator.PausedTicks++;
                OnAnimatorUpdate(entity, ref animator);
                return;
            }
            
            animator.ActiveEvents = false;
            
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
            
            if (frameIndex == animator.PrevFrameIndex)
                return;
            
            animator.PrevFrameIndex = frameIndex;
            OnUpdateEvents(ref animator, frameIndex);
            OnAnimationFrameIndexUpdate(entity, ref animator, frameIndex);
        });
    }

    private void OnUpdateEvents(ref Animator animator, int frameIndex)
    {
        var clip = animator.CurrentClip!;
        if (!clip.EventsByFrameIndex.TryGetValue(frameIndex, out var events))
            return;
        
        animator.CurrentEvents = events;
        animator.ActiveEvents = true;
    }

    public bool IsEventTriggered(Entity entity, string eventName)
    {
        ref var animator = ref GetComponent<Animator>(entity);
        return IsEventTriggered(ref animator, eventName);
    }
    
    public bool IsEventTriggered(ref Animator animator, string eventName)
    {
        if (!animator.ActiveEvents)
            return false;

        return animator.CurrentEvents.Find(d => d.Name == eventName) != null;
    }

    public bool TryGetEvent(Entity entity, string eventName, [MaybeNullWhen(false)] out EventData eventData)
    {
        ref var animator = ref GetComponent<Animator>(entity);
        return TryGetEvent(ref animator, eventName, out eventData);
    }
    
    public bool TryGetEvent(ref Animator animator, string eventName, [MaybeNullWhen(false)] out EventData eventData)
    {
        eventData = null;
        
        if (!animator.ActiveEvents)
            return false;

        eventData = animator.CurrentEvents.Find(d => d.Name == eventName);
        return eventData != null;
    }
    
    public void Play(Entity entity, string clipName)
    {
        Play(entity, clipName, null);
    }
    
    public void Play(Entity entity, string clipName, bool? loop)
    {
        var clip = GetAnimationClip(clipName);
        Play(entity, clip, loop);
    }
    
    public void Play(Entity entity, AnimationClip clip, bool? loop)
    {
        ref var animator = ref !HasComponent<Animator>(entity) ? ref AddComponent<Animator>(entity) : ref GetComponent<Animator>(entity);
        animator.CurrentClip = clip;
        animator.CurrentClipName = clip.Name;
        animator.StartTick = (int)GetCurrentTick();
        animator.PausedTicks = 0;
        animator.IsPlaying = true;
        animator.IsPaused = false;
        animator.IsLooping = loop ?? clip.Loop;
        animator.PrevFrameIndex = -1;
        animator.ActiveEvents = false;
        OnAnimatorUpdate(entity, ref animator);
    }
    
    public void Stop(Entity entity)
    {
        ref var animator = ref !HasComponent<Animator>(entity) ? ref AddComponent<Animator>(entity) : ref GetComponent<Animator>(entity);
        animator.IsPlaying = false;
        animator.IsPaused = false;
        animator.PausedTicks = 0;
        animator.PrevFrameIndex = -1;
        animator.ActiveEvents = false;
        OnAnimatorUpdate(entity, ref animator);
    }
    
    public void Pause(Entity entity)
    {
        ref var animator = ref !HasComponent<Animator>(entity) ? ref AddComponent<Animator>(entity) : ref GetComponent<Animator>(entity);
        animator.IsPaused = true;
        OnAnimatorUpdate(entity, ref animator);
    }
    
    public void Resume(Entity entity)
    {
        ref var animator = ref !HasComponent<Animator>(entity) ? ref AddComponent<Animator>(entity) : ref GetComponent<Animator>(entity);
        animator.IsPaused = false;
        OnAnimatorUpdate(entity, ref animator);
    }
}