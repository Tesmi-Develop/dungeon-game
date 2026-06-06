using Client.Data;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Attributes.Engine;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem, Scene(SceneType.Game)]
public class AnimationClipSyncerSystem : BaseSystem
{
    [Dependency] private AnimationContainer _animationContainer = null!;
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<Animator>();
    
    [Priority(EcsPriority.High)]
    public override void GameUpdate(long tick, long predictTick)
    {
        Query(_queryMeta).With<Animator>((_, ref animator) =>
        {
            if (animator.CurrentClipName == string.Empty || !_animationContainer.TryGetClip(animator.CurrentClipName, out var clip))
                return;

            animator.CurrentClip = clip;
        });
    }
}