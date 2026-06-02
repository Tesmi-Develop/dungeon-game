using Hypercube.Core.Resources;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Helpers;
using Server.Systems.PlayerSystems;
using Shared;
using Shared.Components;
using Shared.Data;
using Shared.Events;
using Shared.Extensions;
using Shared.SharedSystemRealisation;
using Shared.Systems;

namespace Server.Systems.GameplaySystems;

[EcsSystem]
public class MapHandler : SharedMapHandlerSystem
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    [Dependency] private readonly SpawnerPlayerCharacterSystem _characterSystem = null!;
    [Dependency] private readonly GameplayEntitySystem _gameplayEntitySystem = null!;
    private PrototypeStorage _prototypeStorage = null!;
    private readonly QueryMeta _meta = new QueryMeta().WithAll<MapComponentTag>();
    
    public override void Initialize()
    {
        base.Initialize();
        _prototypeStorage = _resourceManager.Load<PrototypeStorage>("/prototypes.json");
        var gameplayEntity = _gameplayEntitySystem.Get();
        AddComponent(gameplayEntity, new GameMap());
        
        Subscribe<Stage, StageUpdated>((entity, ref stage, ref stageUpdated) =>
        {
            var info = Maps.GetMapInfoByStage(stage.StageNumber);
            SetMap(Maps.GetMapRenderByStage(stage.StageNumber), info, Vector2.Zero, Vector2.One / 2, new Vector2(info.Scale));
        });
    }

    public void SetMap(MapRender mapRender, MapInfo mapInfo, Vector2 position, Vector2 anchor, Vector2 scale)
    {
        ClearMapComponents();
        
        var gameplayEntity = _gameplayEntitySystem.Get();
        ref var gameMap = ref GetComponent<GameMap>(gameplayEntity);
    
        mapRender.Load(World, _prototypeStorage, position, anchor, scale);
        
        gameMap.MapName = mapInfo.Name;
        gameMap.Position = position;
        gameMap.Anchor = anchor;
        gameMap.Scale = scale;
        NetworkHelper.MakeDirty<GameMap>(World, gameplayEntity);
        
        Logger.Info($"Loaded Map {mapInfo.Name}");
    }

    public void ClearMapComponents()
    {
        foreach (var e in World.CollectEntities(Query(_meta), []))
            EntityDestroy(e);
    }
}