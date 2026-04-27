using System.Collections.Concurrent;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Data;

namespace Server.Systems.Network;

public class ClientConnection
{
    private readonly NetPeer _peer;
    public readonly ConcurrentQueue<byte[]> IncomingPackets = new();

    public ClientConnection(NetPeer peer)
    {
        _peer = peer;
    }

    public void Send(byte[] data, DeliveryMethod deliveryMethod)
    {
        if (_peer.ConnectionState != ConnectionState.Connected) 
            return;
        
        _peer.Send(data, deliveryMethod);
    }
    
    public void OnPacketReceive(NetDataReader reader)
    {
        var packetType = (PacketType)reader.PeekByte();
        var packet = reader.GetRemainingBytes();
        if (packet is null || packet.Length == 0)
            return;
        
        if (packetType == PacketType.Ping)
        {
            Send(packet, DeliveryMethod.Unreliable);
            return;
        }
        
        IncomingPackets.Enqueue(packet); 
    }
}