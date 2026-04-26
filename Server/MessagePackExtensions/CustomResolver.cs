using System.Collections.Frozen;
using MessagePack;
using MessagePack.Formatters;
using Server.MessagePackExtensions.Formatters;

namespace Server.MessagePackExtensions;

public class CustomResolver : IFormatterResolver
{
    public static readonly IFormatterResolver Instance = new CustomResolver();

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(FrozenDictionary<,>))
        {
            var genericArgs = typeof(T).GetGenericArguments();
            var formatterType = typeof(Server.MessagePackExtensions.Formatters.FrozenDictionaryFormatter<,>).MakeGenericType(genericArgs);
            return (IMessagePackFormatter<T>)Activator.CreateInstance(formatterType)!;
        }
        
        if (typeof(T) == typeof(Type))
        {
            return (IMessagePackFormatter<T>)new TypeAsStringFormatter();
        }

        return null!;
    }
}