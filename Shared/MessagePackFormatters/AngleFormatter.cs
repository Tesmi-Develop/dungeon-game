using Hypercube.Mathematics;
using MessagePack;
using MessagePack.Formatters;

namespace Shared.MessagePackFormatters;

public sealed class AngleFormatter : IMessagePackFormatter<Angle>
{
    public void Serialize(ref MessagePackWriter writer, Angle value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(1);
        writer.Write(value.Theta);
    }

    public Angle Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil) return default;
        
        var count = reader.ReadArrayHeader();
        return count != 1 ? 
            throw new MessagePackSerializationException("Invalid Angle format!") : 
            new Angle (reader.ReadDouble());
    }
}