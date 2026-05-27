using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Shared.Attributes;
using Shared.Components.Enemies;

namespace Shared.Components;

[SyncComponent]
public partial struct SpriteReference : IComponent
{
    public string DefaultTexturePatch = string.Empty;
    public Vector2 Scale = Vector2.One;
    
    public SpriteReference()
    {
    }
}