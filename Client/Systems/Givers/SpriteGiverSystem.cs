using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Shared.Components;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Client.Systems.Givers;

[EcsSystem]
public class SpriteGiverSystem : BaseSystem
{
    private Query _query = null!;
    private List<Entity> _entities = [];

    public override void Initialize()
    {
        _query = GetQuery().WithAll<SpriteReference>().WithNone<SpriteComponent>().Build();
    }

    public override void Update(FrameEventArgs args)
    {
        foreach (var e in World.CollectEntities(_query, _entities))
        {
            Console.WriteLine(1);
            ref var reference = ref GetComponent<SpriteReference>(e);
            AddComponent(e, new SpriteComponent {  Path = reference.Path == string.Empty ? "/textures/default.png" : reference.Path });
        }
    }
}