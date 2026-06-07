using Hypercube.Ecs.Lifetime;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Server.Utilities;
using Shared.Components.EngineComponents;
using Shared.Components.MapComponents;
using Shared.SharedSystemRealisation;

namespace Server.Systems.MapSystems;

[EcsSystem]
public class EnemySpawnerSystem : BaseSystem
{
    public override void Initialize()
    {
        Subscribe<EnemySpawner, AddedEvent>((entity, ref component, ref args) =>
        {
            if (!HasComponent<NetworkTransform>(entity))
                return;
            
            var transform = GetComponent<NetworkTransform>(entity);
            SpawnEnemy(component.Enemy, transform.Position);
        });
    }

    private void SpawnEnemy(string enemyName, Vector2 position)
    {
        switch (enemyName)
        {
            case "Melee":
                Prefabs.CreateMeleeEnemy(World, position);
                break;
        }
    }
}