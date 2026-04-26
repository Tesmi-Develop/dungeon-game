namespace EcsServer.Data;

public enum PacketType : byte
{
    Sync,
    Hydrate,
    ComponentTable,
    Request,
}