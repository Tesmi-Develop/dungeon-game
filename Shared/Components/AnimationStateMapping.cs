using Hypercube.Ecs.Components;

namespace Shared.Components;

public struct AnimationStateMapping : IComponent
{
    public Dictionary<Type, string> Animations = [];

    public AnimationStateMapping()
    {
    }
}