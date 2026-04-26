namespace Shared.Data;

public enum PacketType : byte
{
    Sync,
    Hydrate,
    ComponentTable,
    Request,
}