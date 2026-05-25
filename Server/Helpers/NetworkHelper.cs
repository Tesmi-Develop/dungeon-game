using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Hypercube.Ecs.Queries;
using Server.Components;
using Shared.Data;
using Shared.Extensions;

namespace Server.Helpers;

public static class NetworkHelper
{
    private static Query? _networkComponentQueryMeta;
    private static NetworkMetadata? _networkComponents;
    
    public static void MakeDirty<T>(World world, Entity entity) where T : struct, IComponent
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
        _networkComponentQueryMeta ??= new QueryBuilder(world).WithAll<NetworkMetadata>().Build();
        
        if (_networkComponents is not null)
            return _networkComponents.Value;

        var entity = world.GetFirstEntity(_networkComponentQueryMeta);
        if (entity == Entity.Invalid)
            throw new Exception("Not found list with network components.");
            
        _networkComponents = world.Get<NetworkMetadata>(entity);
        return _networkComponents.Value;
    }
    
    public static Type GetNetworkComponentById(World world, int id)
    {
        var networkComponents = GetNetworkComponentMetadata(world);
        return networkComponents.ComponentsById[id];
    }

    public static List<InputData> GetInputData<T>(World world, Entity clientEntity)
    {
        if (!world.Has<ClientData>(clientEntity))
            throw new ArgumentException("Entity is not client");

        var clientData = world.Get<ClientData>(clientEntity);
        var inputs = new List<InputData>();

        foreach (var inputData in clientData.Inputs)
        {
            if (inputData.Input is T)
            {
                inputs.Add(inputData);
            }
        }

        return inputs;
    }

    public static bool TryGetInputFromTick<T>(World world, Entity clientEntity, long tick, out T? input)
    {
        input = default;
        
        if (!world.Has<ClientData>(clientEntity))
            throw new ArgumentException("Entity is not client");

        ref var clientData = ref world.Get<ClientData>(clientEntity);

        var inputs = clientData.InputsWithTick[tick % clientData.InputsWithTick.Length];
        if (!inputs.TryGetValue(typeof(T), out var inputData))
            return false;
        
        if (inputData.Tick != tick)
            return false;
        
        if (inputData.Input is not T dataInput)
            return false;
        
        input = dataInput;
        return true;
    }
}