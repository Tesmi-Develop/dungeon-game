using Hypercube.Core.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;

namespace Client.Systems.PredictSystems;

public class FieldHistorySystem : EntitySystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    private Query _query = null!;
    
    public override void Initialize()
    {
        _query = GetQuery().WithAll<EntityPredictHistory>().Build();
    }

    public void WriteEntitiesHistory(long serverTick)
    {
        _query.With<EntityPredictHistory>((entity, ref history) =>
        {
            foreach (var componentId in history.Buffers.Keys)
                NetworkFactory.WriteComponentHistory(componentId, entity, World, serverTick, ref history);
        });
    }
}