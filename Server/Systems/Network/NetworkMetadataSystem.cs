using System.Collections.Frozen;
using Hypercube.Utilities.Helpers;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.SharedSystemRealisation;

namespace Server.Systems.Network;

[EcsSystem(EcsPriority.High)]
public class NetworkMetadataSystem : BaseSystem
{
    [Priority(EcsPriority.High)]
    public override void BeforeInitialize()
    {
        var entity = World.Create();
        var registry = new NetworkMetadata();
        CollectNetworkComponents(ref registry);
        CollectNetworkRequests(ref registry);
        
        World.Add(entity, registry);
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