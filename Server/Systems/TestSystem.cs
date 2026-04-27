using System.Xml.XPath;
using Arch.Core;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Random;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Events;
using Server.Helpers;
using Shared.Components;

namespace Server.Systems;

[EcsSystem]
public class TestSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    private Rect2 _arenaSize = new Rect2(-200, 200, 200, -200);
    private Xoshiro256 _random = new(new Random().Next());
    private QueryDescription _query;
    private QueryDescription _query2;
    private float _speed = .1f;
    private int counter = 0;
    
    public override void PostInitialize()
    {
        for (int i = 0; i < 10; i++)
        {
            var entity2 = world.Create();
            world.Add(entity2, new Transform() { Position = Vector2.One });
            world.Add(entity2, new TargetLocation() { Location = Vector2.One });
        }
        _query = new QueryDescription().WithAll<Transform, TargetLocation>();
    }

    private Vector2 GetRandomPosition()
    {
        var newPoint = new Vector2(_random.NextFloat(_arenaSize.Left, _arenaSize.Right), _random.NextFloat(_arenaSize.Bottom, _arenaSize.Top));
        return newPoint;
    }

    public override void Update(float deltaTime)
    {
        world.Query(in _query, (Entity entity, ref Transform transform, ref TargetLocation location) =>
        {
            if ((location.Location - transform.Position).Length <= 10)
            {
                location.Location = GetRandomPosition();
            }

            transform.Position = new Vector2(
                HyperMath.MoveTowards(transform.Position.X, location.Location.X, deltaTime * _speed), 
                HyperMath.MoveTowards(transform.Position.Y, location.Location.Y, deltaTime * _speed));
            
            NetworkHelper.MakeDirty<Transform>(world, entity);
        });
    }
}