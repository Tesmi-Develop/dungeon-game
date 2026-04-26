namespace Shared.Data;

public struct Packet
{
    public PacketType PacketType { get; init; }
    public DeliveryType DeliveryType { get; init; }
    public ReadOnlyMemory<byte> Data { get; init; }
}