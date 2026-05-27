using Hypercube.Ecs;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Server.Components;
using Shared.Components;
using Shared.Components.EngineComponents;
using Shared.Data;
using Shared.Extensions;

namespace Server.Extensions;

public static class WorldExtensions
{
    public record struct CollisionInfo()
    {
        public Vector2 Position = Vector2.Zero;
        public Angle Rotation = Angle.Zero;
        public float Radius = 0;
        public Vector2 Offset = Vector2.Zero;
        public Vector2 Size = Vector2.One;
    }
    
    public record struct DamagePayload()
    {
        public int Damage = 0;
        public int LifeTicks = 1;
    }
    
    public static Entity CreateDamageableCollision(this World world, CollisionInfo collisionInfo, DamagePayload damagePayload, FractionType fractionType)
    {
        var entity = world.Create();

        world.Add<DeferredTag>(entity);
        world.Add(entity, new NetworkTransform { Position = collisionInfo.Position });
        world.Add(entity, new Damage { Value = damagePayload.Damage });

        if (collisionInfo.Radius > 0 && collisionInfo.Size != Vector2.Zero)
            throw new ArgumentException("You cannot specify both size and radius simultaneously.");
        
        if (collisionInfo.Radius > 0)
            world.AddCollision(entity, collisionInfo.Radius, collisionInfo.Offset, isTrigger: true, rotation: collisionInfo.Rotation);
        else
            world.AddCollision(entity, collisionInfo.Size, collisionInfo.Offset, isTrigger: true, rotation: collisionInfo.Rotation);
        
        if (damagePayload.LifeTicks > 0)
            world.Add(entity, new Lifetime
            {
                RemainingTicks =  damagePayload.LifeTicks,
            });
        
        world.Add(entity, new Fraction { Value = fractionType });
        return entity;
    }
}