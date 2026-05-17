using Client.Utilities;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics;
using Hypercube.Physics.Mathematics;
using Hypercube.Physics.Shapes;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.SharedSystemRealisation;
using Shared.Systems.Collisions;

namespace Client.Systems.Collisions;

[EcsSystem]
public sealed class CollisionDebugRenderSystem : BaseSystem, IPatch
{
    public const bool DebugDrawNearbyCollisions = true;
    public const bool DebugDrawChunksNum = false;

    [Dependency] private readonly IResourceManager _resourceManager = null!;

    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly CollisionSystem _collision = null!;
    [Dependency] private readonly CollisionWorldSystem _collisionWorld = null!;

    private readonly List<Entity> _entities = [];
    
    private Query _query = null!;
    private Query _queryNonPlayer = null!;

    private Font _font = null!;

    public int Priority => -1000;

    public override void Initialize()
    {
        _font = _resourceManager.Load<Font>("/fonts/OpenSans.ttf", [("size", 18)]);
        
        _query = GetQuery().WithAll<NetworkTransform, HitboxComponent, PlayerCharacter>().Build();
        _queryNonPlayer = GetQuery().WithAll<NetworkTransform, HitboxComponent>().Build();
    }
    
    public void Draw(IRenderContext renderer, DrawPayload payload)
    {
        if (DebugDrawNearbyCollisions)
            DrawNearbyCollisions(renderer, payload);
        
        if (DebugDrawChunksNum)
            DrawChunksNums(renderer, payload);
    }

    private void DrawNearbyCollisions(IRenderContext renderer, DrawPayload payload)
    {
        _query.With((Entity entity, ref HitboxComponent hitboxComponent, ref PlayerCharacter playerCharacter) =>
        {
            if (playerCharacter.ClientId != _gameClient.Id)
                return;
            
            if (hitboxComponent.GridIndex is not { } gridIndex)
                return;
            
            _entities.Clear();
            _collisionWorld.GetNearby(gridIndex, _entities);
            
            foreach (var targetEntity in _entities)
            {
                if (!TryGetComponent(targetEntity, out HitboxComponent targetHitboxComponent))
                    continue;
                
                if (targetHitboxComponent.Shape.Type != ShapeType.Polygon)
                    continue;
                
                if (!TryGetComponent(targetEntity, out NetworkTransform targetNetworkTransformComponent))
                    continue;
                
                var transform = new Transform(targetNetworkTransformComponent.Position);
                renderer.DrawShapePolygonLine(targetHitboxComponent.Shape.Shape.Polygon, transform, Color.Red);
            }
        });
    }

    private void DrawChunksNums(IRenderContext renderer, DrawPayload payload)
    {
        _queryNonPlayer.With((Entity entity, ref NetworkTransform transform, ref HitboxComponent hitboxComponent) =>
        {
            renderer.DrawText(hitboxComponent.GridIndex?.ToString() ?? "null", _font, transform.Position - Vector2.UnitY * 24f, Color.Gray, 1f, Vector2.Half);
        });
    }
}