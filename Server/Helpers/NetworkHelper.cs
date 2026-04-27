using Arch.Core;
using Server.Extensions;
using Server.Components;

namespace Server.Helpers;

public static class NetworkHelper
{
    private static readonly QueryDescription NetworkComponentQueryMeta = new QueryDescription().WithAll<NetworkMetadata>();
    private static NetworkMetadata? _networkComponents;
    
    public static void MakeDirty<T>(World world, Entity entity) where T : struct
    {
        if (!world.Has<T>(entity))
            throw new Exception($"Component {typeof(T).Name} does not exist.");
        
        var component = world.Get<T>(entity);
        var networkComponents = GetNetworkComponentMetadata(world);
        
        if (!networkComponents.ComponentsByType.TryGetValue(component.GetType(), out var id))
            throw new Exception($"Component {typeof(T).Name} ");

        if (!world.Has<Dirty>(entity))
            world.Add(entity, new Dirty());

        ref var dirty = ref world.Get<Dirty>(entity);
        dirty.ComponentIds.Add(id);
    }

    public static NetworkMetadata GetNetworkComponentMetadata(World world)
    {
        if (_networkComponents is not null)
            return _networkComponents.Value;

        var entity = world.GetFirstEntity(NetworkComponentQueryMeta);
        if (entity == Entity.Null)
            throw new Exception("Not found list with network components.");
            
        _networkComponents = world.Get<NetworkMetadata>(entity);
        return _networkComponents.Value;
    }
    
    public static Type GetNetworkComponentById(World world, int id)
    {
        var networkComponents = GetNetworkComponentMetadata(world);
        return networkComponents.ComponentsById[id];
    }
}