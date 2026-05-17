using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.Components.Enemies;
using Shared.SharedSystemRealisation;

namespace Server.Systems.EnemySystems;

[EcsSystem]
public class StateUpdaterSystem : BaseSystem
{
    private QueryMeta _meta = new QueryMeta().WithAll<State>();
    
    [Priority(EcsPriority.StateUpdater)]
    public override void AfterGameUpdate(long tick, long predictTick)
    {
        Query(_meta).With((Entity entity, ref State state) =>
        {
            if (state.StateType == state.PrevStateType)
                return;

            state.PrevStateType = state.StateType;
            NetworkHelper.MakeDirty<State>(World, entity);
        });
    }
}