using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Systems;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics;
using Hypercube.Physics.Shapes.Structs;
using Hypercube.Utilities.Dependencies;

namespace Rainclipse;

public sealed class TestCollideSystem : PatchEntitySystem
{
    [Dependency] private readonly IResourceManager _resource = null!;
    
    private Font _font = null!;
    
    public override void Initialize()
    {
        _font = _resource.Load<Font>("/fonts/OpenSans.ttf", [("size", 24)]);
    }

    public override void Draw(IRenderContext renderer, DrawPayload payload)
    {
        var time = (float) DateTime.Now.TimeOfDay.TotalSeconds;
        
        var circleA = new ShapeCircle { Center = new Vector2(0, 0), Radius = 50 };
        var circleB = new ShapeCircle { Center = new Vector2(0, 0), Radius = 50 };
        
        var transformA = new Transform(new Vector2(200 + float.Sin(time) * 100, 200 + float.Cos(time) * 50));
        var transformB = new Transform(new Vector2(300 + float.Sin(time * 1.5f) * 80, 220 + float.Cos(time * 1.5f) * 60));

        var manifold = Collide.Circles(circleA, transformA, circleB, transformB);
        
        renderer.DrawCircle(transformA.Position + circleA.Center, circleA.Radius, Color.Blue, outline: true);
        renderer.DrawCircle(transformB.Position + circleB.Center, circleB.Radius, Color.Red, outline: true);
        
        if (!manifold.IsEmpty)
        {
            var contactPoint = manifold.Points[0];
            var normalEnd = contactPoint.Point + manifold.Normal * 50;
            renderer.DrawLine(contactPoint.Point, normalEnd, Color.Yellow);

            renderer.DrawText(
                $"Collision!\nNormal: {manifold.Normal}\nSeparation: {manifold.Points[0].Separation:F2}",
                _font,
                transformA.Position + new Vector2(0, -70),
                Color.White
            );
            
            return;
        }

        renderer.DrawText("No collision", _font, transformA.Position + new Vector2(0, -70), Color.White);
    }
}