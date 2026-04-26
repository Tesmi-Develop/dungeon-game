using EcsServer.Events;
using EcsServer.Systems.Network;

namespace EcsServer.Components.Events;

public struct ClientConnected : IEvent
{
    public ClientConnection ClientConnection;
}