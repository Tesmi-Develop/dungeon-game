using Client.Utilities;
using Hypercube.Utilities.Dependencies;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class TestAnimator : BaseSystem
{
    [Dependency] private readonly AnimatorSystem _animator = null!;
    public override void Initialize()
    {
        /*var entity = EntityCreate();
        AddComponent<NetworkTransform>(entity);
        _animator.Play(entity, "enemy/Idle");*/
    }
}