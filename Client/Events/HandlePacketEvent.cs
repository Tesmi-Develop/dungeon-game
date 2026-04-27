using Hypercube.Ecs.Events;
using Shared.Data;

namespace Client.Events;

public struct HandlePacketEvent : IEvent
{
    public Packet Packet;
    public bool Handled;
}