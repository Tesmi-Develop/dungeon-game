using Hypercube.Ecs;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;

namespace Shared.SharedSystemRealisation;

public partial class SharedSystem
{
    [Dependency] public readonly World World = null!;
    [Dependency] public readonly ILogger Logger = null!;
    
    public virtual void BeforeInitialize() { }
    public virtual void Initialize() { }
    public virtual void AfterInitialize() { }
    
    public virtual void BeforeGameUpdate(long tick, long predictTick) { }
    public virtual void GameUpdate(long tick, long predictTick) { }
    public virtual void AfterGameUpdate(long tick, long predictTick) { }
}