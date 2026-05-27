using Server.Components.Events;
using Server.Helpers;
using Server.Utilities;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Server.Systems.GameSystems;

[EcsSystem]
public class DamageSystem : BaseSystem
{
    public override void Initialize()
    {
        Subscribe<Health, TookDamage>((entity, ref health, ref damageArg) =>
        {
            health.Current = Math.Clamp(health.Current - damageArg.Value, 0, health.Max);
            Console.WriteLine(health.Current);
            NetworkHelper.MakeDirty<Health>(World, entity);
        });
    }
}