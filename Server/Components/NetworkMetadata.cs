using System.Collections.Frozen;
using MessagePack;

namespace Server.Components;

public struct NetworkMetadata
{
    [Key(0), IgnoreMember] public FrozenDictionary<Type, int> ComponentsByType;
    [Key(1), IgnoreMember] public FrozenDictionary<int, Type> ComponentsById;
    
    [Key(2), IgnoreMember] public FrozenDictionary<Type, int> RequestsByType;
    [Key(3), IgnoreMember] public FrozenDictionary<int, Type> RequestsById;
}