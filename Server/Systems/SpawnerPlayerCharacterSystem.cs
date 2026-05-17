using Hypercube.Ecs;
using Hypercube.Ecs.Events;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Components.Events;
using Server.Utilities;
using Shared.Components;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem]
public class SpawnerPlayerCharacterSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IEventBus _eventBus = null!;
    private Query _queryDescription = null!;
    private Query _queryPlayers = null!;

    public override void BeforeInitialize()
    {
        _queryDescription = GetQuery().WithAll<PlayerSpawner, NetworkTransform>().Build();
        _queryPlayers = GetQuery().WithAll<ClientData>().WithNone<ControlledEntity>().Build();
    }

    public override void Initialize()
    {
        _eventBus.Subscribe((Entity playerEntity, ref ClientData playerData, ref NewEntityClient _) =>
        {
            InitiateSpawnPlayerCharacters();
        });
        
        _eventBus.Subscribe((Entity clientEntity, ref ClientData clientData, ref ClientEntityRemoved _) =>
        {
            DespawnPlayerCharacter(clientEntity);
        });
    }

    public void InitiateSpawnPlayerCharacters()
    {
        var entity = World.GetFirstEntity(_queryDescription);
        if (entity == Entity.Invalid)
        {
            _logger.Warning("Not found player spawner entity");
            return;
        }
        
        _queryPlayers.With((Entity clientEntity, ref ClientData clientData) =>
        {
            var networkTransform = World.Get<NetworkTransform>(entity);
            SpawnPlayerCharacter(clientEntity, networkTransform.Position, ref clientData);
        });
    }

    public void SpawnPlayerCharacter(Entity playerEntity, Vector2 position, ref ClientData playerData)
    {
        var characterEntity = World.Create();
        World.Add(playerEntity, new ControlledEntity { Reference = characterEntity });
        
        World.Add(characterEntity, new NetworkTransform { Position = position });
        World.Add(characterEntity, new SpriteReference { DefaultTexturePatch = string.Empty }); //TODO player sprite
        World.Add(characterEntity, new Speed { Value = 4f });
        World.Add(characterEntity, new PlayerCharacter { ClientId = playerData.Id });
        World.AddCollision(characterEntity, new Vector2(32, 32), isTrigger: true);
    }

    public void DespawnPlayerCharacter(Entity clientEntity)
    {
        if (!World.Has<ControlledEntity>(clientEntity))
            return;
        
        var controlled = World.Get<ControlledEntity>(clientEntity);
        var clientData = World.Get<ClientData>(clientEntity);
        if (!World.Validate(controlled.Reference))
            return;
        
        World.Delete(controlled.Reference);
    }
}