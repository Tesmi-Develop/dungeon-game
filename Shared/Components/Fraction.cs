using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Data;

namespace Shared.Components;

[SyncComponent]
public partial struct Fraction : IComponent
{
    public FractionType Value;
}