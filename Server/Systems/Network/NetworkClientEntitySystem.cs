using Hypercube.Ecs;
using Hypercube.Ecs.Events;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Server.Components;
using Server.Components.Events;
using Server.Utilities;
using Shared.Data;
using Shared.Helpers;
using Shared.SharedSystemRealisation;

namespace Server.Systems.Network;

[EcsSystem(EcsPriority.High)]
public class NetworkClientEntitySystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    [Dependency] private readonly Logger _logger = null!;
    [Dependency] private readonly NetworkServer _networkServer = null!;
    
    private Query _query = null!;
    private readonly Dictionary<long, Entity> _clients = [];

    public override void Initialize()
    {
        _query = GetQuery().WithAll<ClientData>().Build();
        _eventBus.Subscribe((ref ClientConnected args) =>
        {
            RegisterClientEntity(args.ClientConnection);
        });
        
        _eventBus.Subscribe((ref ClientDisconnected args) =>
        {
            if (!_clients.TryGetValue(args.ClientConnection.Id, out var entity))
                return;
            
            _eventBus.Raise(entity, ref World.Get<ClientData>(entity), new ClientEntityRemoved());
            DestroyClientEntity(args.ClientConnection.Id);
        }, (int)EventBusPriority.Lowest);
    }

    public Entity GetClientEntity(long clientId)
    {
        return  _clients[clientId];
    }

    private void DestroyClientEntity(long clientId)
    {
        if (!_clients.TryGetValue(clientId, out var entity))
            return;
        
        World.Delete(entity);
        _clients.Remove(clientId);
        _logger.Debug($"Client {clientId} has been destroyed");
    }

    private void RegisterClientEntity(ClientConnection clientConnection)
    {
        var entity = World.Create();
        var clientData = new ClientData { ClientConnection = clientConnection, Id = clientConnection.Id };

        for (var i = 0; i < clientData.InputsWithTick.Length; i++)
            clientData.InputsWithTick[i] = new();
        
        World.Add(entity, clientData);
        _clients.Add(clientConnection.Id, entity);
        _eventBus.Raise(entity, ref World.Get<ClientData>(entity), new NewEntityClient { ClientEntity = entity });
    }

    private void HandleIncomingPackets(Entity entity, ref ClientData clientData)
    {
        while (clientData.ClientConnection.IncomingPackets.TryDequeue(out var packet))
        {
            clientData.IncomingPackets.Enqueue(packet);
        }
    }

    private void HandleOutgoingPackets(ref ClientData clientData)
    {
        while (clientData.PendingPackets.TryDequeue(out var packet))
        {
            var finalData = new byte[1 + 8 + packet.Data.Length];
            finalData[0] = (byte)packet.PacketType;
            MessagePackHelper.WriteInt64(new Span<byte>(finalData, 1, 8), packet.Tick);
            packet.Data.CopyTo(finalData.AsMemory(1 + 8));
            
            if (packet.DeliveryType != DeliveryMethod.Unreliable)
                _logger.Debug($"Sending packet {packet.PacketType}");
            
            clientData.ClientConnection.Send(finalData, packet.DeliveryType);
        }
    }
    
    public override void BeforeGameUpdate(long tick, long _)
    {
        _query.With((Entity entity, ref ClientData clientData) =>
        {
           HandleIncomingPackets(entity, ref clientData);
           HandleOutgoingPackets(ref clientData);
        });
    }
}