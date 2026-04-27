using System.Collections.Concurrent;
using LiteNetLib;
using LiteNetLib.Utils;

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
        var myData = reader.GetRemainingBytes();
        if (myData is null || myData.Length == 0)
            return; 
        
        IncomingPackets.Enqueue(myData); 
    }
}