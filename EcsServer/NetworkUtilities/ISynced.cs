using System.Buffers;
using MessagePack;

namespace EcsServer.NetworkUtilities;

public interface ISynced
{
    void Serialize(IBufferWriter<byte> writer, MessagePackSerializerOptions? options);
}
