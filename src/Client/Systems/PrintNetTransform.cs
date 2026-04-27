using Hypercube.Core.Ecs;
using Hypercube.Ecs.Queries;
using Shared.Components;

namespace Client.Systems;

public class PrintNetTransform : EntitySystem
{
    private Query _query;
    public override void Initialize()
    {
        _query = GetQuery().WithAny<Transform>().Build();
    }

    public override void Update(float deltaTime)
    {
        _query.With<Transform>((_, ref transform) =>
        {
            Console.WriteLine(transform.Position);
        });
    }
}