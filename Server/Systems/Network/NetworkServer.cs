using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Hypercube.Ecs.Events;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Server.Components.Events;
using Server.Utilities;
using Shared.Data;
using Shared.Helpers;
using Shared.SharedSystemRealisation;

namespace Server.Systems.Network;

[EcsSystem(EcsPriority.High)]
public class NetworkServer : BaseSystem, INetEventListener
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly Time _time = null!;
    
    private readonly NetManager _server;
    private readonly ConcurrentDictionary<NetPeer, ClientConnection> _connections = [];
    private readonly ConcurrentQueue<IEvent> _eventQueue = [];
    private readonly Dictionary<Type, Action<object>> _globalEventCache = [];
    private long _freeClientId;

    public NetworkServer()
    {
        _server = new NetManager(this);
    }

    public long GetServerTime()
    {
        return _time.Stopwatch.ElapsedMilliseconds;
    }

    public override void Initialize()
    {
        const int port = 5000;
        _server.Start(port);
        _ = ReceiveCycle();
        _logger.Info($"Server started on port {port}");
    }

    public override void GameUpdate(long tick, long _)
    {
        while (_eventQueue.TryDequeue(out var eventInstance))
            RaiseEvent(eventInstance);
    }

    private void RaiseEvent(IEvent eventArgs)
    {
        var type = eventArgs.GetType();

        if (!_globalEventCache.TryGetValue(type, out var action))
        {
            var busType = _eventBus.GetType();
            var method = busType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .First(m => m.Name == nameof(IEventBus.Raise) && 
                            m.GetGenericArguments().Length == 1 &&
                            m.GetParameters()[0].ParameterType.IsByRef == false)
                .MakeGenericMethod(type);

            // (obj) => Raise<ActualType>((ActualType)obj)
            action = (object ev) => method.Invoke(_eventBus, [ev]);
            _globalEventCache[type] = action;
        }

        action(eventArgs);
    }

    private async Task ReceiveCycle()
    {
        while (true)
        {
            _server.PollEvents();
            await Task.Delay(1);
        }
    }

    private void SendHandshake(ClientConnection connection)
    {
        Span<byte> buffer = stackalloc byte[1 + 8 + 8];
        buffer[0] = (byte)PacketType.Handshake;
        MessagePackHelper.WriteInt64(buffer.Slice(1), GetServerTime());
        MessagePackHelper.WriteInt64(buffer.Slice(9), connection.Id);
        
        connection.Peer.Send(buffer.ToArray(), DeliveryMethod.ReliableOrdered);
    }

    public void OnPeerConnected(NetPeer peer)
    {
        var newConnection = new ClientConnection(peer, _freeClientId++);
        _connections.TryAdd(peer, newConnection);
        _eventQueue.Enqueue(new ClientConnected { ClientConnection = newConnection });
        
        SendHandshake(newConnection);
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
        if (!_connections.TryGetValue(peer, out var client) || reader.AvailableBytes < 1)
        {
            reader.Recycle();
            return;
        }

        var type = (PacketType)reader.GetByte();

        switch (type)
        {
            case PacketType.Ping:
            {
                if (reader.AvailableBytes < 8)
                {
                    reader.Recycle();
                    return;
                }

                var clientSendTime = reader.GetLong();
                var serverTime = GetServerTime();

                // response packet: [type][clientTime][serverTime]
                Span<byte> buffer = stackalloc byte[1 + 8 + 8];

                buffer[0] = (byte)PacketType.Ping;

                MessagePackHelper.WriteInt64(buffer.Slice(1), clientSendTime);
                MessagePackHelper.WriteInt64(buffer.Slice(9), serverTime);

                client.Send(buffer.ToArray(), DeliveryMethod.Unreliable);
                break;
            }

            default:
                client.OnPacketReceive(reader, type);
                break;
        }

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