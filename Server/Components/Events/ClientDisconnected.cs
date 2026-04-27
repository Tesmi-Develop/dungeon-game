using Server.Events;
using Server.Systems.Network;

namespace Server.Components.Events;

public struct ClientDisconnected : IEvent
{
    public ClientConnection ClientConnection;
}