using Hypercube.Core.Ecs;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.Extensions;

namespace Client.Systems;

public class SpawnLocalCharacterSystem : EntitySystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    
    public override void Initialize()
    {
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            //SpawnPlayerCharacter(_gameClient.Id);
        });
    }
    
    private void SpawnPlayerCharacter(long id)
    {
        var characterEntity = World.Create();
        
        World.Add(characterEntity, new NetworkTransform { Position = Vector2.Zero });
        World.Add(characterEntity, new Speed { Value = 4f });
        World.Add(characterEntity, new PlayerCharacter { ClientId = id });
        World.AddCollision(characterEntity, new Vector2(32, 32));
    }
}