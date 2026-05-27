using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Utilities.Dependencies;
using Shared.SharedSystemRealisation;

namespace Client.InternalSystems;

[EcsSystem]
public class EmulatorServerUpdateSystem : BaseSystem
{
    [Dependency] private readonly EntrySystem _entrySystem = null!;
    private const bool IsEnable = false;
    private const int TickRate = 60;
    private const double TickInterval = 1.0 / TickRate;
    private const int MaxCatchUpTicks = 5;

    private double _accumulator;
    private long _currentTick;

    public override void Update(FrameEventArgs eventArgs)
    {
        if (!IsEnable)
            return;
        
        _accumulator += eventArgs.Delta.TotalSeconds;

        var ticksProcessed = 0;
        
        while (_accumulator >= TickInterval)
        {
            SimulateTick(_currentTick);

            _currentTick++;
            _accumulator -= TickInterval;
            ticksProcessed++;
            
            if (ticksProcessed >= MaxCatchUpTicks)
            {
                _accumulator = 0;
                break;
            }
        }
    }

    private void SimulateTick(long tick)
    {
        _entrySystem.InvokeGameUpdatePhase(tick, tick);
    }
}