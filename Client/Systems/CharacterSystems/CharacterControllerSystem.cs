using Client.Data;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Shared.Components;
using Shared.Components.Commands;
using Shared.SharedSystemRealisation;

namespace Client.Systems.CharacterSystems;

[EcsSystem]
public class CharacterControllerSystem : BaseSystem
{
    [Dependency] private readonly InputStorage _inputStorage = null!;
    [Dependency] private readonly NetworkHelper _networkHelper = null!;
    [Dependency] private readonly GameClient _gameClient = null!;
    
    private Query _query = null!;
    
    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, PlayerCharacter, Speed>().Build();
    }

    public override void GameUpdate(long serverTick, long predictTick)
    {
        _query.With<NetworkTransform, PlayerCharacter, Speed>((entity, ref transform, ref playerCharacter, ref speed) =>
        {
            if (_gameClient.Id != playerCharacter.ClientId)
                return;
            
            var direction = new Vector2();
        
            if (_inputStorage.HasInput(Input.MoveUp))
                direction += Vector2.UnitY;
        
            if (_inputStorage.HasInput(Input.MoveDown))
                direction -= Vector2.UnitY;
        
            if (_inputStorage.HasInput(Input.MoveLeft))
                direction -= Vector2.UnitX;
        
            if (_inputStorage.HasInput(Input.MoveRight))
                direction += Vector2.UnitX;

            if (direction != Vector2.Zero)
            {
                transform.Position += direction * speed.Value;
                _networkHelper.SendInputIfPredicting(new MoveRequest { Direction = direction }, DeliveryMethod.Unreliable);
            }
        });
    }
}