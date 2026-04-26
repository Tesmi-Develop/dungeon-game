using Arch.Core;
using Hypercube.Utilities.Dependencies;

namespace Server;

public class BaseSystem
{
    [Dependency] public readonly World world = null!;
    
    public virtual void PreInitialize() { }
    public virtual void Initialize() { }
    public virtual void PostInitialize() { }
    public virtual void BeforeUpdate(float deltaTime) { }
    public virtual void Update(float deltaTime) { }
    public virtual void AfterUpdate(float deltaTime) { }
}