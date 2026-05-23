using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.States;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.CharacterSystems;

[EcsSystem]
public class EndMovingStateTransitionSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<MovingDirection, State>().WithAll<Moving>();

    [Priority(EcsPriority.StateUpdater)]
    public override void GameUpdate(long tick, long predictTick)
    {
        With(_queryMeta, (Entity e, ref MovingDirection direction, ref State state) =>
        {
            if (direction.Direction != Vector2.Zero)
                return;
            
            World.SetState<Idle>(e);
        });
    }
}