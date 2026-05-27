using Hypercube.Ecs;

namespace Shared.Extensions;

public static class EntityExtensions
{
    public static long GetFullMask(this Entity entity)
    {
        return (long)entity.Version << 32 | entity.Id;
    }
}