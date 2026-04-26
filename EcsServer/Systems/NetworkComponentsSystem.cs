using System.Collections.Frozen;
using EcsServer.Attributes;
using EcsServer.Components;
using Hypercube.Utilities.Helpers;

namespace EcsServer.Systems;

[EcsSystem(EcsPriority.High)]
public class NetworkComponentsSystem : BaseSystem
{
    public override void PreInitialize()
    {
        var entity = world.Create();
        var registry = new NetworkRegistry();
        CollectNetworkComponents(ref registry);
        CollectNetworkRequests(ref registry);
        
        world.Add(entity, registry);
    }

    private void CollectNetworkComponents(ref NetworkRegistry registry)
    {
        var componentsByType = new Dictionary<Type, int>();
        var componentsById = new Dictionary<int, Type>();
        var id = 0;
        
        foreach (var (type, _) in ReflectionHelper.GetAllTypesWithAttribute<SyncComponentAttribute>())
        {
            componentsByType.Add(type, id);
            componentsById.Add(id, type);
            id++;
        }
        
        registry.ComponentsByType = componentsByType.ToFrozenDictionary();
        registry.ComponentsById = componentsById.ToFrozenDictionary();
    }
    
    private void CollectNetworkRequests(ref NetworkRegistry registry)
    {
        var componentsByType = new Dictionary<Type, int>();
        var componentsById = new Dictionary<int, Type>();
        var id = 0;
        
        foreach (var (type, _) in ReflectionHelper.GetAllTypesWithAttribute<RequestComponentAttribute>())
        {
            componentsByType.Add(type, id);
            componentsById.Add(id, type);
            id++;
        }
        
        registry.RequestsByType = componentsByType.ToFrozenDictionary();
        registry.RequestsById = componentsById.ToFrozenDictionary();
    }
}