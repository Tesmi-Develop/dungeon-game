using Arch.Core;
using Hypercube.Utilities.Dependencies;

namespace EcsServer;

public class BaseSystem
{
    [Dependency] public readonly World world;
    
    public virtual void PreInitialize() { }
    public virtual void Initialize() { }
    public virtual void PostInitialize() { }
    public virtual void BeforeUpdate(float deltaTime) { }
    public virtual void Update(float deltaTime) { }
    public virtual void AfterUpdate(float deltaTime) { }
}