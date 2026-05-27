using System.Reflection;
using Client.Utilities;
using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Shared.Components;
using Shared.Helpers;
using Shared.SharedSystemRealisation;

namespace Client.Systems.PredictSystems;

[EcsSystem]
public class PredictHelper : BaseSystem
{
    public const int Capacity = 60;
    
    public void PredictField<T>(Entity entity, string fieldName) where T : struct, IComponent 
    {
        if (!World.Has<EntityPredictHistory>(entity))
            World.Add(entity, new EntityPredictHistory());
    
        ref var data = ref World.Get<EntityPredictHistory>(entity);
        var componentId = NumeratorGenerator.GetId(typeof(T));
    
        if (!data.Buffers.TryGetValue(componentId, out var fields))
        {
            fields = [];
            data.Buffers.Add(componentId, fields);
        }
        
        var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (fieldInfo is null)
            throw new ArgumentException($"Field '{fieldName}' not found in component {typeof(T).Name}");
        
        if (!IsUnmanaged(fieldInfo.FieldType))
            throw new ArgumentException($"Field '{fieldName}' is not a unmanage type");
        
        var buffer = FieldHistoryHelper.CreateFieldBuffer<T>(fieldInfo, Capacity); 
        fields.Add(buffer);
    }
    
    private bool IsUnmanaged(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
            return true;
        
        if (!type.IsValueType)
            return false;
        
        return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .All(f => IsUnmanaged(f.FieldType));
    }
}