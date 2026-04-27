using Client.Components;
using Hypercube.Core.Ecs;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Shared.Components;

namespace Client.Systems;

public class SpriteGiverSystem : EntitySystem
{
    private Query _query;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<Transform>().WithNone<SpriteComponent>().Build();
    }

    public override void Update(float deltaTime)
    {
        _query.With<Transform>((e, ref transform) =>
        {
            AddComponent(e, new TransformComponent());
            AddComponent(e, new InterpolationComponent());
            AddComponent(e, new SpriteComponent() { Path = "/textures/default.png" });
        });
    }
}