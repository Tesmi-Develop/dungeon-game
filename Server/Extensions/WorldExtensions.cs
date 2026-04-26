using Arch.Core;

namespace Server.Extensions;

public static class WorldExtensions 
{
    public static Entity GetFirstEntity(this World world, in QueryDescription queryDescription)
    {
        var query = world.Query(in queryDescription);
        foreach (ref var chunk in query)
        {
            if (chunk.Count > 0)
            {
                return chunk.Entity(0);
            }
        }
        
        return Entity.Null;
    }
}