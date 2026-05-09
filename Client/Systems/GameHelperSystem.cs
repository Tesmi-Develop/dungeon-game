using Hypercube.Core.Ecs;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class GameHelperSystem : EntitySystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    private Query _localCharacterQuery = null!;

    public override void Initialize()
    {
        _localCharacterQuery = GetQuery().WithAll<NetworkTransform, PlayerCharacter>().Build();
    }

    public Entity GetLocalCharacter()
    {
        foreach (var e in _localCharacterQuery)
        {
            var playerCharacter = GetComponent<PlayerCharacter>(e);
            if (playerCharacter.ClientId == _gameClient.Id)
                return e;
        }

        return Entity.Invalid;
    }
}