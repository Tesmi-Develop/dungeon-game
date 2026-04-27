using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using Server;
using Shared.Data;

namespace Client;

public class GameClient : INetEventListener
{
    [Dependency] private readonly ILogger _logger = null!;
    private NetManager _client = null!;
    private NetPeer? _serverPeer;
    public readonly ConcurrentQueue<Packet> Packets = [];
    private Stopwatch _stopwatch = Stopwatch.StartNew();
    public long Ping { get; private set; }
    private ArrayBufferWriter<byte> _bufferWriter = new();
    private LatencyProxy _proxy;
    
    public GameClient()
    {
        _proxy = new LatencyProxy(this, 50, 150, 20);
        _client = new NetManager(_proxy);
        _client.UpdateTime = 0;
    }

    public void Start()
    {
        _client.Start();
        _ = ReceiveCycle();
        _ = ReceivePing();
    }
    
    public async Task ConnectAsync(string ip, int port)
    {
        _client.Connect(ip, port, "DeathBall");
    }
    
    public void Send(byte[] data, DeliveryMethod deliveryMethod)
    {
        if (_serverPeer is null || _serverPeer.ConnectionState != ConnectionState.Connected) 
            return;
        
        _serverPeer.Send(data, deliveryMethod);
    }

    private void UpdatePing()
    {
        _bufferWriter.Clear();
        var writer = new MessagePackWriter(_bufferWriter);
        writer.Write((byte)PacketType.Ping);
        writer.WriteInt64(_stopwatch.ElapsedMilliseconds);
        writer.Flush();
        
        Send(_bufferWriter.WrittenMemory.ToArray(), DeliveryMethod.Unreliable);
    }

    private async Task ReceivePing()
    {
        while (true)
        {
            UpdatePing();
            await Task.Delay(1000);
        }
    }

    private async Task ReceiveCycle()
    {
        while (true)
        {
            _proxy?.Update();
            _client.PollEvents();
            await Task.Delay(1);
        }
    }

    private void HandleIncomingData(byte[] data, DeliveryMethod deliveryType)
    {
        if (data.Length == 0)
            return;
        
        var packetType = (PacketType)data[0];
        var payload = new Memory<byte>(data, 1, data.Length - 1);

        if (packetType == PacketType.Ping)
        {
            var reader = new MessagePackReader(payload);
            var current =  _stopwatch.ElapsedMilliseconds;
            var last = reader.ReadInt64();
            Ping = current - last;
            return;
        }
        
        Packets.Enqueue(new Packet
        {
            PacketType = packetType, 
            Data = payload,
            DeliveryType = deliveryType,
        });
    }

    public void OnPeerConnected(NetPeer peer)
    {
        _serverPeer = peer;
        _logger.Info($"Connected to Server. Address: {_serverPeer.Address}");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        throw new NotImplementedException();
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        var data = reader.GetRemainingBytes();
        if (data is null || data.Length == 0)
            return;
        
        HandleIncomingData(data, deliveryMethod);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        
    }
}