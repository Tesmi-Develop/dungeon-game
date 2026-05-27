using Client.Systems.PredictSystems;
using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.SharedSystemRealisation;

namespace Client.Systems.Givers;

[EcsSystem]
public class CharacterGiverPredictSystem : BaseSystem
{
    [Dependency] private readonly PredictHelper _predictHelper = null!;
    [Dependency] private readonly GameClient _gameClient = null!;
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, PlayerCharacter>().WithNone<EntityPredictHistory>().Build();
    }

    public override void Update(FrameEventArgs args)
    {
        _query.With<NetworkTransform, PlayerCharacter>((entity, ref _, ref playerCharacter) =>
        {
            if (playerCharacter.ClientId != _gameClient.Id)
                return;
            
            _predictHelper.PredictField<NetworkTransform>(entity, nameof(NetworkTransform.Position));
        });
    }
}