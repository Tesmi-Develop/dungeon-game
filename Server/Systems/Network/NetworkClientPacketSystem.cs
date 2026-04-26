using System.Buffers;
using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using MessagePack;
using Server.Components;
using Server.Components.Events;
using Server.Events;
using Shared.Data;

namespace Server.Systems.Network;

[EcsSystem]
public class NetworkClientPacketSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    [Dependency] private readonly Logger _logger = null!;
    private readonly QueryDescription _query = new QueryDescription().WithAll<ClientData>();
    private readonly ArrayBufferWriter<byte> _bufferWriter = new(1024);

    public override void Initialize()
    {
        _eventBus.Subscribe((ref ClientConnected args) =>
        {
            RegisterClientEntity(args.ClientConnection);
        });
    }

    private void RegisterClientEntity(ClientConnection clientConnection)
    {
        var entity = world.Create();
        world.Add(entity, new ClientData { ClientConnection = clientConnection});
        
        _eventBus.Raise<ClientData, NewEntityClient>(entity, new NewEntityClient { ClientEntity = entity });
    }

    private void HandleIncomingPackets(Entity entity, ref ClientData clientData)
    {
        while (clientData.ClientConnection.IncomingPackets.TryDequeue(out var packet))
        {
            try
            {
                var reader = new MessagePackReader(packet);
                var packetTypeInt = reader.ReadByte();
                var packetType = (PacketType)packetTypeInt;
                    
                clientData.IncomingPackets.Enqueue(new Packet
                {
                    PacketType = packetType, 
                    Data= new Memory<byte>(packet, (int)reader.Consumed, packet.Length - (int)reader.Consumed),
                });
            }
            catch (Exception e)
            {
                _logger.Warning($"Failed to parse packet from Entity({entity.Id}), Size: {packet.Length}, Message: {e.Message}");
            }
        }
    }

    private void HandleOutgoingPackets(ref ClientData clientData)
    {
        while (clientData.PendingPackets.TryDequeue(out var packet))
        {
            _bufferWriter.Clear();
            
            var messagePackWriter =  new MessagePackWriter(_bufferWriter);
            messagePackWriter.Write((byte)packet.PacketType);
            messagePackWriter.WriteRaw(packet.Data.Span);
            messagePackWriter.Flush();
            
            switch (packet.DeliveryType)
            {
                case DeliveryType.Reliable:
                    clientData.ClientConnection.SendTcp(_bufferWriter.WrittenMemory.ToArray());
                    break;
                case DeliveryType.Unreliable:
                    clientData.ClientConnection.SendUdp(_bufferWriter.WrittenMemory.ToArray());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    public override void Update(float deltaTime)
    {
        world.Query(in _query, (Entity entity, ref ClientData clientData) =>
        {
           HandleIncomingPackets(entity, ref clientData);
           HandleOutgoingPackets(ref clientData);
        });
    }
}