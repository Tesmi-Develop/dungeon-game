using Arch.Core;

namespace Server.Extensions;

public static class EntityExtensions
{
    public static long GetFullMask(this Entity entity)
    {
        return ((long)entity.Version << 32) | (uint)entity.Id;
    }
}