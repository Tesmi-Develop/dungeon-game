using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent(invokeEventWhenDirty: true)]
public partial struct Health
{
    public int Max;
    public int Current;
}