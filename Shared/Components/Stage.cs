using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct Stage : IComponent
{
    public uint StageNumber;
}