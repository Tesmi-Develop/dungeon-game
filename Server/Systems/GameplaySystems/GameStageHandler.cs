using Hypercube.Utilities.Dependencies;
using Server.Utilities;
using Shared.Components;
using Shared.Events;
using Shared.SharedSystemRealisation;

namespace Server.Systems.GameplaySystems;

[EcsSystem]
public class GameStageHandler : BaseSystem
{
    [Dependency] private readonly GameplayEntitySystem _gameplayEntitySystem = null!;
    
    public override void BeforeInitialize()
    {
        var entity = _gameplayEntitySystem.Get();
        AddComponent(entity, new Stage { StageNumber = 0 });
    }

    public override void AfterInitialize()
    {
        SetStage(1);
    }

    public void SetStage(uint stageNumber)
    {
        var entity = _gameplayEntitySystem.Get();
        ref var stageComp = ref GetComponent<Stage>(entity);
        
        var previousStage = stageComp.StageNumber;
        stageComp.StageNumber = stageNumber;
        
        Raise(entity, ref stageComp, new StageUpdated { Current =  stageNumber, Previous = previousStage });
    }
}