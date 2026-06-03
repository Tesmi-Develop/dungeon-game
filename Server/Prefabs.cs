using Hypercube.Ecs;
using Hypercube.Mathematics.Vectors;
using Shared.Components;
using Shared.Components.Enemies;
using Shared.Components.Enemies.EnemyTags;
using Shared.Components.EngineComponents;
using Shared.Components.States;
using Shared.Data;
using Shared.Extensions;

namespace Server;

public static class Prefabs
{
    public static Entity CreateMeleeEnemy(World world, Vector2 position)
    {
        var enemy = world.Create();
        world.Add(enemy, new NetworkTransform { Position = position});
        world.Add(enemy, new MovingDirection());
        world.Add(enemy, new Target { TargetAcquisitionRadius = 200, TargetRetentionRadius = 300 });
        world.Add(enemy, new AttackInfo { MaxTargetRange = 40, AttackSize = new Vector2(36, 28), Damage = 1 });
        world.Add(enemy, new Speed { Value = 1f });
        world.Add(enemy, new EnemyTag());
        world.Add(enemy, new AttackerTag());
        world.Add(enemy, new PlayerTargetTag());
        world.Add(enemy, new Health { Current = 10, Max = 10 });
        world.Add(enemy, new Fraction { Value = FractionType.Enemies });
        world.Add(enemy, new ControlRotationByDirection());
        world.Add(enemy, new AnimationStateMapping { Animations =
        {
            [typeof(Idle)] = "enemy/Idle",
            [typeof(Moving)] = "enemy/Movement",
            [typeof(Attacking)] = "enemy/Attacking",
            [typeof(Died)] = "enemy/Died"
        } });
        world.AddCollision(enemy, new Vector2(32, 32), isTrigger: true);
        world.SetState<Idle>(enemy);

        return enemy;
    }

    public static Entity CreatePlayer(World world, Vector2 position, long playerId)
    {
        var characterEntity = world.Create();
        
        world.Add(characterEntity, new NetworkTransform { Position = position });
        world.Add(characterEntity, new MovingDirection());
        world.Add(characterEntity, new AttackInfo { MaxTargetRange = 40, AttackSize = new Vector2(36, 28), Damage = 1 });
        world.Add(characterEntity, new Speed { Value = 4f });
        world.Add(characterEntity, new PlayerCharacter { ClientId = playerId });
        world.Add(characterEntity, new Health { Current = 10, Max = 10 });
        world.Add(characterEntity, new Fraction { Value = FractionType.Players });
        world.Add(characterEntity, new AnimationStateMapping { Animations =
        {
            [typeof(Idle)] = "player/Idle",
            [typeof(Moving)] = "player/Movement",
            [typeof(Attacking)] = "player/Attacking",
            [typeof(Died)] = "player/Died"
        } });
        world.AddCollision(characterEntity, new Vector2(32, 32), isTrigger: true);
        world.SetState<Idle>(characterEntity);
        world.Add(characterEntity, new Light()
        {
            Radius = 150,
            Intensity = 1,
            Falloff = 0.3f
        });

        return characterEntity;
    }
}