using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Data;

namespace Client;

public class GameClient : INetEventListener
{
    [Dependency] private readonly ILogger _logger = null!;
    private NetManager _client = null!;
    private NetDataWriter _writer = new();
    private NetPeer? _serverPeer;
    public readonly ConcurrentQueue<Packet> Packets = [];

    public GameClient()
    {
        _client = new NetManager(this);
    }

    public void Start()
    {
        _client.Start();
        _ = ReceiveCycle();
    }
    
    public async Task ConnectAsync(string ip, int port)
    {
        _client.Connect(ip, port, "DeathBall");
    }
    
    public async Task Send(byte[] data, DeliveryMethod deliveryMethod)
    {
        if (_serverPeer is null || _serverPeer.ConnectionState != ConnectionState.Connected) 
            return;
        
        _serverPeer.Send(data, deliveryMethod);
    }

    private async Task ReceiveCycle()
    {
        while (true)
        {
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