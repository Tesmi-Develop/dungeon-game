using Hypercube.Core.Resources;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Systems.PlayerSystems;
using Server.Utilities;
using Shared;
using Shared.SharedSystemRealisation;

namespace Server.Systems;

[EcsSystem()]
public class LoadArenaTest : BaseSystem
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly SpawnerPlayerCharacterSystem _characterSystem = null!;
    private MapHandler _mapHandler = null!;
    private PrototypeStorage _prototypeStorage = null!;
    
    public override void Initialize()
    {
        _mapHandler = new MapHandler("/TiledMaps/Arena1/arena1.tmj");
        _mapHandler.Compile(_resourceManager);
        
        _prototypeStorage = _resourceManager.Load<PrototypeStorage>("/prototypes.json");
        Task.Run(async () =>
        {
            //await Task.Delay(5000);
            
        });
        
        _mapHandler.Load(World, _prototypeStorage, Vector2.Zero, Vector2.One / 2, new Vector2(2));
        _logger.Debug("Loaded Arena1");
        _characterSystem.InitiateSpawnPlayerCharacters();
    }
}