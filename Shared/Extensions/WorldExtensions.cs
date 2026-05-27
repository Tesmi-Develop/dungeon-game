using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes;
using Hypercube.Physics.Shapes.Structs;
using Shared.Components.EngineComponents;

namespace Shared.Extensions;

public static class WorldExtensions 
{
    extension(World world)
    {
        public List<Entity> CollectEntities(Query query, List<Entity> entities)
        {
            if (entities.Count > 0)
                entities.Clear();
            
            query.ForEach(entities.Add);
            return entities;
        }
        
        public List<Entity> CollectEntities(QueryMeta meta, List<Entity> entities)
        {
            if (entities.Count > 0)
                entities.Clear();
            
            world.Query(meta).ForEach(entities.Add);
            return entities;
        }
        
        public Entity GetFirstEntity(Query query)
        {
            foreach (var e in query)
                return e;
        
            return Entity.Invalid;
        }

        public int CountEntities(Query query)
        {
            var count = 0;
            foreach (var unused in query)
                count++;

            return count;
        }
        
        public void AddCollision(Entity entity, Vector2 size, Vector2? offset = null, bool isTrigger = false, bool isStatic = false, Angle? rotation = null)
        {
            offset ??= Vector2.Zero;
            rotation ??= Angle.Zero;
            
            world.Add(entity, new CollisionComponent
            {
                Shape = new ShapeUnionTyped
                {
                    Shape = new ShapeUnion
                    {
                        Polygon = ShapePolygon.CreateRectangle(size / 2f)
                    },
                    Type = ShapeType.Polygon,
                },
                Size = size,
                Radius = 0,
                Rotation = rotation.Value,
                IsTrigger = isTrigger,
                IsStatic = isStatic,
                Offset = offset.Value,
            });
        }
        
        public void AddCollision(Entity entity, float radius, Vector2? offset, bool isTrigger = false, bool isStatic = false, Angle? rotation = null)
        {
            offset ??= Vector2.Zero;
            rotation ??= Angle.Zero;
            
            world.Add(entity, new CollisionComponent
            {
                Shape = new ShapeUnionTyped
                {
                    Shape = new ShapeUnion
                    {
                        Circle = new ShapeCircle
                        {
                            Radius = radius
                        }
                    },
                    Type = ShapeType.Circle,
                },
                Size = Vector2.Zero,
                Radius = radius,
                Rotation = rotation.Value,
                IsTrigger = isTrigger,
                IsStatic = isStatic,
                Offset = offset.Value,
            });
        }
    }
}