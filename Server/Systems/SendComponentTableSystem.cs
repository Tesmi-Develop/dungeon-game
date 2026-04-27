using Hypercube.Utilities.Dependencies;
using MessagePack;
using Server.Events;

namespace Server.Systems;

[EcsSystem]
public class SendComponentTableSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    private byte[] _componentTable = [];

    public override void Initialize()
    {
        /*var buffer = new ArrayBufferWriter<byte>(1024);
        var writer = new MessagePackWriter(buffer);
        var metadata = NetworkHelper.GetNetworkComponentMetadata(world);
        
        writer.WriteArrayHeader(2);
        WriteDictionary(ref writer, metadata.ComponentsById);
        WriteDictionary(ref writer, metadata.RequestsById);
        writer.Flush();
        
        _componentTable = buffer.WrittenMemory.ToArray();
        var packet = new Packet
        {
            PacketType = PacketType.Metadata,
            DeliveryType = DeliveryType.Reliable,
            Data = _componentTable
        };*/
    }
    
    private void WriteDictionary(ref MessagePackWriter writer, IDictionary<int, Type> dict)
    {
        writer.WriteMapHeader(dict.Count);
        foreach (var pair in dict)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value.Name);
        }
    }

    private void WriteDictionary(ref MessagePackWriter writer, IDictionary<Type, int> dict)
    {
        writer.WriteMapHeader(dict.Count);
        foreach (var pair in dict)
        {
            writer.Write(pair.Key.Name);
            writer.Write(pair.Value);
        }
    }
}