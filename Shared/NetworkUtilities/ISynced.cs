using System.Buffers;
using Hypercube.Ecs.Components;
using MessagePack;

namespace Shared.NetworkUtilities;

public interface ISynced : IComponent
{
    void Serialize(IBufferWriter<byte> writer, MessagePackSerializerOptions? options);
    void Deserialize(ReadOnlyMemory<byte> sequence, MessagePackSerializerOptions? options);
}
