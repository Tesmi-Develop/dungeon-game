using Hypercube.Core.Execution.LifeCycle;
using Shared.SharedSystemRealisation;

namespace Client.Utilities;

public class BaseSystem : SharedSystem
{
    public virtual void BeforeUpdate(FrameEventArgs eventArgs) {}
    public virtual void Update(FrameEventArgs eventArgs) {}
    public virtual void AfterUpdate(FrameEventArgs eventArgs) {}
}