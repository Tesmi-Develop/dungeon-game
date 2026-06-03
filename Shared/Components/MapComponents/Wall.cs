using Hypercube.Ecs.Components;
using Hypercube.Physics.Shapes;

namespace Shared.Components.MapComponents;

public struct Wall : IComponent
{
    public ShapeType ShapeType = ShapeType.Polygon;

    public Wall()
    {
    }
}