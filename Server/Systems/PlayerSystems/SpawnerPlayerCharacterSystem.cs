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
using Shared.Components.Enemies;
using Shared.Components.EngineComponents;
using Shared.Components.States;
using Shared.Data;
using Shared.Extensions;
using Shared.SharedSystemRealisation;

namespace Server.Systems.PlayerSystems;

[EcsSystem]
public class SpawnerPlayerCharacterSystem : BaseSystem
{
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
            SpawnPlayerCharacterInSpawnPoint(playerEntity);
        });
        
        _eventBus.Subscribe((Entity clientEntity, ref ClientData clientData, ref ClientEntityRemoved _) =>
        {
            DespawnPlayerCharacter(clientEntity);
        });
    }

    public void SpawnPlayerCharacterInSpawnPoint(Entity playerEntity)
    {
        var entity = World.GetFirstEntity(_queryDescription);
        if (entity == Entity.Invalid || !World.Has<NetworkTransform>(entity))
        {
            Logger.Warning("Not found player spawner entity");
            return;
        }
        
        var spawnPoint = World.Get<NetworkTransform>(entity).Position;
        SpawnPlayerCharacter(playerEntity, spawnPoint);
    }

    public void SpawnPlayerCharacter(Entity playerEntity, Vector2 position)
    {
        var playerData = World.Get<ClientData>(playerEntity);
        var characterEntity = Prefabs.CreatePlayer(World, position, playerData.Id);
        AddComponent(playerEntity, new ControlledEntity { Reference = characterEntity });
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