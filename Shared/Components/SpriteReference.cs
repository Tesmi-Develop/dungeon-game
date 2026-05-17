using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Components.Enemies;

namespace Shared.Components;

[SyncComponent]
public partial struct SpriteReference : IComponent
{
    public string DefaultTexturePatch = string.Empty;
    public Dictionary<StateType, string> Animations = [];
    
    public SpriteReference()
    {
    }
}