using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components.Enemies;

public partial struct State : IComponent
{
    public Type Current;
    public Type Prev;
    public bool FrozenState;
}