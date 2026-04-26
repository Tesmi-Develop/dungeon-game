using Arch.Core;
using EcsServer.Events;

namespace EcsServer.Components.Events;

public struct NewEntityClient : IEvent
{
    public Entity ClientEntity;
}