

using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components.Enemies;

[SyncComponent]
public partial struct State : IComponent
{
    public StateType StateType;
    [NonSynced]
    public StateType PrevStateType;
}