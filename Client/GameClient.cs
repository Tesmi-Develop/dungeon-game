using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Hypercube.Core.Execution.Timing;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Shared;
using Shared.Data;
using Shared.Helpers;

namespace Client;

public class GameClient : INetEventListener
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly ITime _time = null!;

    private const int JitterTick = 3;
    private readonly NetManager _client;
    private NetPeer? _serverPeer;
    public readonly ConcurrentQueue<Packet> Packets = [];
    public long Ping => _serverPeer is null ? -1 : _ping;
    private long _ping;
    public long LatencyTick { get; private set; }
    private ArrayBufferWriter<byte> _bufferWriter = new();
    private readonly LatencyProxy _proxy;
    private double _timeOffset;
    private const float TimeSmooth = 0.1f;
    private static readonly double TickRate = Config.TickRate;
    private readonly double _tickMs = 1000.0 / TickRate;
    
    public long Id => _serverPeer is null ? -1 : _id;
    private long _id = -1;
    public bool Connected => _serverPeer is not null && _serverPeer.ConnectionState == ConnectionState.Connected;
    
    public GameClient()
    {
        _proxy = new LatencyProxy(this, 0, 0, 0);
        _client = new NetManager(_proxy)
        {
            UpdateTime = 0
        };
    }

    public long GetServerTick()
    {
        var localTime = _time.Elapsed.TotalMilliseconds;
        var synchronizedTime = localTime + _timeOffset;
        
        return (long)Math.Floor(synchronizedTime / _tickMs);
    }
    
    public double GetServerTickDouble()
    {
        var localTime = _time.Elapsed.TotalMilliseconds;
        var synchronizedTime = localTime + _timeOffset;
        
        return synchronizedTime / _tickMs;
    }
    
    public double GetLocalTime()
    {
        return _time.ElapsedMilliseconds;
    }

    public long GetPredictServerTick(long currentTick)
    {
        return currentTick + LatencyTick + JitterTick;
    }
    
    public long GePredictServerTickOffset()
    {
        return LatencyTick + JitterTick;
    }
    
    public long GetServerTime()
    {
        return (long)(GetLocalTime() + _timeOffset);
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
    
    public void Send(PacketType packetType, byte[] data, DeliveryMethod deliveryMethod)
    {
        if (_serverPeer is null || _serverPeer.ConnectionState != ConnectionState.Connected)
        {
            _logger.Warning("Server is not connected");
            return;
        }

        var finalData = new byte[data.Length + 1];
        finalData[0] = (byte)packetType;
        Buffer.BlockCopy(data, 0, finalData, 1, data.Length);
        
        _serverPeer.Send(finalData, deliveryMethod);
    }

    private void UpdatePing()
    {
        if (_serverPeer is null || _serverPeer.ConnectionState != ConnectionState.Connected)
            return;
        
        Span<byte> buffer = stackalloc byte[1 + 8];
        buffer[0] = (byte)PacketType.Ping;
        MessagePackHelper.WriteInt64(buffer.Slice(1), (long)GetLocalTime());
        
        _serverPeer.Send(buffer.ToArray(), DeliveryMethod.Unreliable);
    }

    private async Task ReceivePing()
    {
        while (true)
        {
            try
            {
                UpdatePing();
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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

    private void HandleIncomingData(PacketType packetType, long serverTick, byte[] data, DeliveryMethod deliveryType)
    {
        if (deliveryType != DeliveryMethod.Unreliable)
            _logger.Debug($"Got packet type: {packetType}");
        
        Packets.Enqueue(new Packet
        {
            PacketType = packetType, 
            Data = new Memory<byte>(data, 0, data.Length),
            DeliveryType = deliveryType,
            Tick = serverTick
        });
    }

    public void OnPeerConnected(NetPeer peer)
    {
        _serverPeer = peer;
        _logger.Info($"Connected to Server. Address: {_serverPeer.Address}.");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _serverPeer = null;
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        if (reader.UserDataSize == 0)
        {
            _logger.Warning("Received empty packet");
            return;
        }
        
        var packetType = (PacketType)reader.GetByte();

        if (packetType == PacketType.Handshake)
        {
            var serverTime = reader.GetLong();
            _id = reader.GetLong();
            _timeOffset = serverTime - GetLocalTime();
            _logger.Info($"My id {_id}");
            return;
        }
        
        if (packetType == PacketType.Ping)
        {
            var t1 = GetLocalTime();
            var clientSentTime = reader.GetLong();
            var serverTime = reader.GetLong();
            
            var rtt = t1 - clientSentTime;
            var oneWay = rtt / 2.0;
            var targetOffset = (serverTime + oneWay) - t1;
            var diff = targetOffset - _timeOffset;
            
            _ping = (long)(Ping * 0.8 + rtt * 0.2);
            LatencyTick = (long)Math.Floor(oneWay / _tickMs);
            
            if (Math.Abs(diff) > 66)
                _timeOffset = targetOffset;
            else
                _timeOffset += diff / 8.0; 
            
            return;
        }
        
        var serverTick = reader.GetLong();
        var data = reader.GetRemainingBytes();
        HandleIncomingData(packetType, serverTick, data, deliveryMethod);
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