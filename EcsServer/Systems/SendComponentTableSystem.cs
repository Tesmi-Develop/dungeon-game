using System.Buffers;
using Arch.Core;
using EcsServer.Components;
using EcsServer.Components.Events;
using EcsServer.Data;
using EcsServer.Events;
using EcsServer.Helpers;
using Hypercube.Utilities.Dependencies;
using MessagePack;

namespace EcsServer.Systems;

[EcsSystem]
public class SendComponentTableSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus;
    private byte[] _componentTable = [];

    public override void Initialize()
    {
        var buffer = new ArrayBufferWriter<byte>(1024);
        var writer = new MessagePackWriter(buffer);
        var metadata = NetworkHelper.GetNetworkComponentMetadata(world);
        
        writer.WriteArrayHeader(2);
        WriteDictionary(ref writer, metadata.ComponentsById);
        WriteDictionary(ref writer, metadata.RequestsById);
        writer.Flush();
        
        _componentTable = buffer.WrittenMemory.ToArray();
        var packet = new Packet
        {
            PacketType = PacketType.ComponentTable,
            DeliveryType = DeliveryType.Reliable,
            Data = _componentTable
        };
        
        _eventBus.Subscribe((Entity _, ref ClientData playerData, ref NewEntityClient _) =>
        {
            playerData.PendingPackets.Enqueue(packet);
        }, EventBusPriority.Critical);
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