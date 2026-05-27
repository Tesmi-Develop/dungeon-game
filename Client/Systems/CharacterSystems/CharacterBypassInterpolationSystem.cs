using Client.Components;
using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs;
using Hypercube.Utilities.Dependencies;
using Shared.SharedSystemRealisation;

namespace Client.Systems.CharacterSystems;

[EcsSystem]
public class CharacterBypassInterpolationSystem : BaseSystem
{
    [Dependency] private readonly GameHelperSystem _gameHelperSystem = null!;
    
    public override void Update(FrameEventArgs args)
    {
        var entity = _gameHelperSystem.GetLocalCharacter();
        
        if (entity == Entity.Invalid || !HasComponent<Interpolation>(entity))
            return;

        GetComponent<Interpolation>(entity).IsBypass = true;
    }
}