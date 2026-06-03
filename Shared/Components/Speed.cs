using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components;

[SyncComponent]
public partial struct Speed : IComponent
{
    public float Value;
}