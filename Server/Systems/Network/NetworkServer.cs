using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Server.Components.Events;
using Server.Events;

namespace Server.Systems.Network;

[EcsSystem]
public class NetworkServer : BaseSystem, INetEventListener
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    [Dependency] private readonly ILogger _logger = null!;
    private readonly NetManager _server;
    private readonly ConcurrentDictionary<NetPeer, ClientConnection> _connections = [];
    private readonly ConcurrentQueue<IEvent> _eventQueue = [];
        
    public NetworkServer()
    {
        _server = new NetManager(this);
        _server.UpdateTime = 0;
    }

    public override void Initialize()
    {
        const int port = 5000;
        _server.Start(port);
        _ = ReceiveCycle();
    }

    public override void Update(float deltaTime)
    {
        while (_eventQueue.TryDequeue(out var eventInstance))
            _eventBus.Raise(eventInstance);
    }

    private async Task ReceiveCycle()
    {
        while (true)
        {
            _server.PollEvents();
            await Task.Delay(1);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        var newConnection = new ClientConnection(peer);
        _connections.TryAdd(peer, newConnection);
        _eventQueue.Enqueue(new ClientConnected { ClientConnection = newConnection });
        _logger.Info($"New client connected {peer.Address}:{peer.Port}");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (!_connections.TryRemove(peer, out var clientConnection))
            return;
        
        _eventQueue.Enqueue(new ClientDisconnected { ClientConnection = clientConnection });
        _logger.Info($"Client disconnected {peer.Address}:{peer.Port}");
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        _logger.Error($"Network error: {socketError} at {endPoint}");
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        if (_connections.TryGetValue(peer, out var clientConnection))
            clientConnection.OnPacketReceive(reader);
    
        reader.Recycle();
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey("DeathBall");
    }
}