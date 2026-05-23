using Hypercube.Ecs;
using Shared.Components;

namespace Shared.Extensions;

public static class WorldCharacterExtensions
{
    public static bool IsAliveCharacter(this World world, Entity character)
    {
        if (!world.Has<Health>(character))
            return true;
        
        return world.Get<Health>(character).Current > 0;
    }
}