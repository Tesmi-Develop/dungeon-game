using Hypercube.Ecs.Components;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes;
using Shared.Attributes;

namespace Shared.Components.EngineComponents;

[SyncComponent]
public partial struct CollisionComponent : IComponent
{
    public ShapeUnionTyped Shape;
    public bool IsStatic;
    public bool IsTrigger;
    public Vector2 Offset;

    public Vector2 Size;
    public float Radius;
    public Angle Rotation;
    
    [NonSynced]
    public Vector2i? GridIndex;
}