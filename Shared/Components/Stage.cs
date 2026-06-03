using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Attributes.Engine;

namespace Shared.Components;

[SyncComponent]
public partial struct Stage : IComponent
{
    public uint StageNumber;
}