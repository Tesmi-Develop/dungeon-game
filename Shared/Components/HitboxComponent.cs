using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct HitboxComponent : IComponent
{
    public ShapeUnionTyped Shape;
    public bool IsStatic;
    public bool IsTrigger;
    public Vector2 Offset;
    
    [NonSynced]
    public Vector2i? GridIndex;
}