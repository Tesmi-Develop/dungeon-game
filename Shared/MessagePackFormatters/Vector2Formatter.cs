using Hypercube.Mathematics.Vectors;
using MessagePack;
using MessagePack.Formatters;

namespace Shared.MessagePackFormatters;

public sealed class Vector2Formatter : IMessagePackFormatter<Vector2>
{
    public void Serialize(ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write(value.X);
        writer.Write(value.Y);
    }

    public Vector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil) return default;
        
        var count = reader.ReadArrayHeader();
        return count != 2 ? 
            throw new MessagePackSerializationException("Invalid Vector2 format!") : 
            new Vector2 (reader.ReadSingle(), reader.ReadSingle());
    }
}