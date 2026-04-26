using MessagePack;
using MessagePack.Formatters;

namespace EcsServer.Formaters;

public class TypeAsStringFormatter : IMessagePackFormatter<Type>
{
    public void Serialize(ref MessagePackWriter writer, Type value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }
        
        writer.Write(value.Name);
    }

    public Type Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil()) return null;

        var typeName = reader.ReadString();
        return Type.GetType(typeName);
    }
}