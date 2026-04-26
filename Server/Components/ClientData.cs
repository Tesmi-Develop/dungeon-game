using Server.Systems.Network;
using Shared.Data;

namespace Server.Components;

public struct ClientData
{
    public Queue<Packet> PendingPackets = [];
    public Queue<Packet> IncomingPackets = [];
    public ClientConnection ClientConnection = null!;

    public ClientData()
    {
    }
}