using Hypercube.Ecs.Components;
using Server.Systems.Network;
using Shared.Data;

namespace Server.Components;

public struct ClientData : IComponent
{
    public Queue<Packet> PendingPackets = [];
    public Queue<Packet> IncomingPackets = [];
    public ClientConnection ClientConnection = null!;
    public Dictionary<Type, InputData>[] InputsWithTick = new Dictionary<Type, InputData>[60];
    public List<InputData> Inputs = [];
    public long Id = 0;

    public ClientData()
    {
    }
}