using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace Server;

public class LatencyProxy : INetEventListener
{
    private readonly INetEventListener _realListener;
    private readonly Random _random = new();
    private readonly List<DelayedPacket> _delayedPackets = [];
    private readonly Lock _lock = new();
    
    private readonly bool _enableLatency;
    private readonly int _minLatencyMs;
    private readonly int _maxLatencyMs;
    
    private readonly bool _enablePacketLoss;
    private readonly int _packetLossChance;
    
    public LatencyProxy(INetEventListener realListener, int latencyMs = 0)
        : this(realListener, latencyMs, latencyMs, 0)
    {
    }
    
    public LatencyProxy(
        INetEventListener realListener,
        int minLatencyMs,
        int maxLatencyMs,
        int packetLossChance = 0)
    {
        _realListener = realListener;
        _minLatencyMs = minLatencyMs;
        _maxLatencyMs = maxLatencyMs;
        _packetLossChance = packetLossChance;
        
        _enableLatency = minLatencyMs > 0 || maxLatencyMs > 0;
        _enablePacketLoss = packetLossChance > 0;
    }
    
    private struct DelayedPacket
    {
        public NetPeer Peer;
        public NetPacketReader Reader;
        public byte ChannelNumber;
        public DeliveryMethod DeliveryMethod;
        public long DeliveryTick;
    }
    
    public void Update()
    {
        if (!_enableLatency) return;
        
        var currentTick = DateTime.UtcNow.Ticks;
        List<DelayedPacket> toProcess = null;
        
        lock (_lock)
        {
            toProcess = _delayedPackets
                .Where(p => p.DeliveryTick <= currentTick)
                .ToList();
            
            foreach (var packet in toProcess)
                _delayedPackets.Remove(packet);
        }
        
        foreach (var packet in toProcess)
        {
            _realListener.OnNetworkReceive(
                packet.Peer, 
                packet.Reader, 
                packet.ChannelNumber, 
                packet.DeliveryMethod);
        }
    }
    
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        // Симуляция потери пакетов
        if (_enablePacketLoss && _random.Next(100) < _packetLossChance)
        {
            reader.Recycle();
            return;
        }
        
        // Симуляция задержки
        if (_enableLatency)
        {
            var delayMs = _random.Next(_minLatencyMs, _maxLatencyMs + 1);
            var deliveryTick = DateTime.UtcNow.Ticks + (delayMs * TimeSpan.TicksPerMillisecond);
            
            lock (_lock)
            {
                _delayedPackets.Add(new DelayedPacket
                {
                    Peer = peer,
                    Reader = reader,
                    ChannelNumber = channelNumber,
                    DeliveryMethod = deliveryMethod,
                    DeliveryTick = deliveryTick
                });
            }
        }
        else
        {
            _realListener.OnNetworkReceive(peer, reader, channelNumber, deliveryMethod);
        }
    }
    
    // Проксирование остальных методов
    public void OnPeerConnected(NetPeer peer)
    {
        _realListener.OnPeerConnected(peer);
    }
    
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _realListener.OnPeerDisconnected(peer, disconnectInfo);
    }
    
    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        _realListener.OnNetworkError(endPoint, socketError);
    }
    
    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        _realListener.OnNetworkReceiveUnconnected(remoteEndPoint, reader, messageType);
    }
    
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        _realListener.OnNetworkLatencyUpdate(peer, latency);
    }
    
    public void OnConnectionRequest(ConnectionRequest request)
    {
        _realListener.OnConnectionRequest(request);
    }
}