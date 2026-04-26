using Arch.Core;
using Server.Events;

namespace Server.Components.Events;

public struct NewEntityClient : IEvent
{
    public Entity ClientEntity;
}