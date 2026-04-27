namespace Shared.Data;

public enum PacketType : byte
{
    Hydrate,
    Dirty,
    EntitiesDeletion,
    ComponentsDeletion,
    ComponentsAddition,
    Request,
}