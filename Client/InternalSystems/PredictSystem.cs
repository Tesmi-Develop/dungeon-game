using Client.Systems.PredictSystems;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs;
using Hypercube.Utilities.Dependencies;
using Shared.Components;

namespace Client.InternalSystems;

public class PredictSystem : EntitySystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly InputStorage _inputStorage = null!;
    [Dependency] private readonly FieldHistorySystem _fieldHistorySystem = null!;
    [Dependency] private readonly IDependenciesContainer _dependenciesContainer = null!;
    [Dependency] private readonly IRuntimeLoop _runtimeLoop = null!;
    [Dependency] private readonly EntrySystem _entrySystem = null!;
    
    private bool _needRollback;
    private long _lastTick;
    private long _lastProcessedPredictTick;
    private long _missPredictTick = long.MaxValue;
    
    public long PredictTick { get; private set; } = 0;
    public bool IsRollback { get; private set; } = false;

    public override void Initialize()
    {
        _runtimeLoop.Actions.Add(OnUpdate, EngineUpdatePriority.EntitySystemUpdate - 1);
    }

    public void OnUpdate(FrameEventArgs args)
    {
        if (!_gameClient.Connected)
            return;
        
        var serverTick = _gameClient.GetServerTick();
        var tickOffset = _gameClient.GePredictServerTickOffset();

        if (_needRollback)
        {
            StartRollback();
            return;
        }
        
        while (_lastTick < serverTick)
        {
            _lastTick++;
            PredictTick = _lastTick + tickOffset;
            
            if (PredictTick <= _lastProcessedPredictTick)
                continue;

            _lastProcessedPredictTick = PredictTick;
            
            _inputStorage.CaptureActualInputs(PredictTick);
            InvokeServerUpdate(_lastTick, PredictTick);
            _fieldHistorySystem.WriteEntitiesHistory(PredictTick);
        }
    }
    
    private void InvokeServerUpdate(long tick, long predictTick)
    {
        _entrySystem.InvokeGameUpdatePhase(tick, predictTick);
    }
    
    private void StartRollback()
    {
        Logger.Trace($"Got miss predict {_missPredictTick}");
        IsRollback = true;
        ProcessRollback(PredictTick);
        IsRollback = false;
        Logger.Trace($"Got rollback {_lastTick}");
    }
    
    private void ProcessRollback(long currentTick)
    {
        if (currentTick < _missPredictTick)
        {
            Logger.Warning("An attempt to reproduce the future. How?");
            _needRollback = false;
            _missPredictTick = long.MaxValue;
            return;
        }
        
        for (var tick = _missPredictTick + 1; tick <= currentTick; tick++)
        {
            _inputStorage.SetMockInput(tick);
            InvokeServerUpdate(tick, tick);
            _fieldHistorySystem.WriteEntitiesHistory(tick);
        }
        
        _needRollback = false;
        _missPredictTick = long.MaxValue;
    }
    
    public void ReconcileState(Entity entity)
    {
        if (!World.Has<EntityPredictHistory>(entity))
            return;
        
        ref var history = ref World.Get<EntityPredictHistory>(entity);
        if (!history.NeedsRollback)
            return;
        
        _needRollback = true;
        _missPredictTick = Math.Min(history.RollbackTick, _missPredictTick);
        history.NeedsRollback = false;
        history.RollbackTick = 0;
        StartRollback();
    }
}