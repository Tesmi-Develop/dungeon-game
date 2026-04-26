using System.Buffers;
using MessagePack;

namespace Server.NetworkUtilities;

public interface ISynced
{
    void Serialize(IBufferWriter<byte> writer, MessagePackSerializerOptions? options);
}
