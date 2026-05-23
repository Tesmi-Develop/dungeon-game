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
public class MovingStateTransitionSystem : BaseSystem
{
    private readonly QueryMeta _queryMeta = new QueryMeta().WithAll<MovingDirection, State>().WithAny<Idle, Moving>();

    [Priority(EcsPriority.StateUpdater)]
    public override void GameUpdate(long tick, long predictTick)
    {
        With(_queryMeta, (Entity e, ref MovingDirection direction, ref State state) =>
        {
            if (state.FrozenState)
                return;

            if (direction.Direction != Vector2.Zero)
            {
                World.SetState<Moving>(e);
                return;
            }
            
            World.SetState<Idle>(e);
        });
    }
}