using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct Speed : IComponent
{
    public float Value;
}