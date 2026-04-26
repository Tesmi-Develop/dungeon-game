using System.Collections.Frozen;
using MessagePack;
using MessagePack.Formatters;

namespace EcsServer.MessagePackExtensions.Formatters;

public class FrozenDictionaryFormatter<TKey, TValue> : IMessagePackFormatter<FrozenDictionary<TKey, TValue>> 
    where TKey : notnull
{
    public void Serialize(ref MessagePackWriter writer, FrozenDictionary<TKey, TValue> value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }
        
        var formatter = options.Resolver.GetFormatterWithVerify<IEnumerable<KeyValuePair<TKey, TValue>>>();
        formatter.Serialize(ref writer, value, options);
    }

    public FrozenDictionary<TKey, TValue> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }
        
        var intermediate = options.Resolver.GetFormatterWithVerify<Dictionary<TKey, TValue>>()
            .Deserialize(ref reader, options);
        
        return intermediate.ToFrozenDictionary(options.Security.GetEqualityComparer<TKey>());
    }
}