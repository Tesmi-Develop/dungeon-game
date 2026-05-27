using Hypercube.Ecs;
using Hypercube.Utilities.Dependencies;
using Server.Helpers;
using Server.Utilities;
using Shared.Components;
using Shared.ResourcesData;
using Shared.SharedSystemRealisation;
using Shared.Systems;

namespace Server.Systems;

[EcsSystem]
public class AnimatorSystem : SharedAnimatorSystem
{
    [Dependency] private readonly Time _time = null!;
    [Dependency] private readonly AnimationContainer _container = null!;
    
    protected override long GetCurrentTick()
    {
        return _time.Tick;
    }

    protected override AnimationClip GetAnimationClip(string clipName)
    {
        return _container.GetClip(clipName);
    }

    protected override void OnAnimationFrameIndexUpdate(Entity entity, ref Animator animator, int frameIndex)
    {
        // Nothing
    }

    protected override void OnAnimatorUpdate(Entity entity, ref Animator animator)
    {
        NetworkHelper.MakeDirty<Animator>(World, entity);
    }
}