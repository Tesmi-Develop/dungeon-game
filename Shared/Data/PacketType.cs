namespace Shared.Data;

public enum PacketType : byte
{
    Ping,
    Hydrate,
    Dirty,
    EntitiesDeletion,
    ComponentsDeletion,
    ComponentsAddition,
    Request,
}