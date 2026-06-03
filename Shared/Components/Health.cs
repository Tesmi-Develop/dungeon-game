using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components;

[SyncComponent(invokeEventWhenDirty: true)]
public partial struct Health : IComponent
{
    public int Max;
    public int Current;
}