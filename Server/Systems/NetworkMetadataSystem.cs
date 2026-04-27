using System.Collections.Frozen;
using Hypercube.Utilities.Helpers;
using Server.Components;
using Shared.Attributes;

namespace Server.Systems;

[EcsSystem(EcsPriority.High)]
public class NetworkMetadataSystem : BaseSystem
{
    public override void PreInitialize()
    {
        var entity = world.Create();
        var registry = new NetworkMetadata();
        CollectNetworkComponents(ref registry);
        CollectNetworkRequests(ref registry);
        
        world.Add(entity, registry);
    }

    private void CollectNetworkComponents(ref NetworkMetadata metadata)
    {
        var componentsByType = new Dictionary<Type, int>();
        var componentsById = new Dictionary<int, Type>();
        
        foreach (var (type, _) in ReflectionHelper.GetAllTypesWithAttribute<SyncComponentAttribute>())
        {
            var id = NumeratorGenerator.GetId(type);
            componentsByType.Add(type, id);
            componentsById.Add(id, type);
        }
        
        metadata.ComponentsByType = componentsByType.ToFrozenDictionary();
        metadata.ComponentsById = componentsById.ToFrozenDictionary();
    }
    
    private void CollectNetworkRequests(ref NetworkMetadata metadata)
    {
        var componentsByType = new Dictionary<Type, int>();
        var componentsById = new Dictionary<int, Type>();
        
        foreach (var (type, _) in ReflectionHelper.GetAllTypesWithAttribute<RequestComponentAttribute>())
        {
            var id = NumeratorGenerator.GetId(type);
            componentsByType.Add(type, id);
            componentsById.Add(id, type);
        }
        
        metadata.RequestsByType = componentsByType.ToFrozenDictionary();
        metadata.RequestsById = componentsById.ToFrozenDictionary();
    }
}