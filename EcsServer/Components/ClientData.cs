using EcsServer.Data;
using EcsServer.Systems.Network;

namespace EcsServer.Components;

public struct ClientData
{
    public Queue<Packet> PendingPackets = [];
    public Queue<Packet> IncomingPackets = [];
    public ClientConnection ClientConnection;

    public ClientData()
    {
    }
}