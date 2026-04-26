using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using MessagePack;
using Server.Components;
using Shared.Data;
using Exception = System.Exception;

namespace Server.Systems;

[EcsSystem]
public class HandleIncomingClientPacketsSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    private QueryDescription _query;
    
    public override void Initialize()
    {
        _query = new QueryDescription().WithAll<ClientData>();
    }

    public override void Update(float deltaTime)
    {
        world.Query(in _query, (Entity entity, ref ClientData playerData) =>
        {
            while (playerData.IncomingPackets.Count > 0)
            {
                var packet = playerData.IncomingPackets.Dequeue();
                try
                {
                    ProcessPacket(ref packet, entity);
                }
                catch (Exception e)
                {
                    _logger.Warning($"Tried to process packet {packet.PacketType} with exception {e}");
                }
            }
        });
    }

    private void ProcessPacket(ref Packet packet, Entity entity)
    {
        switch (packet.PacketType)
        {
            case PacketType.Request:
                world.Create(new FromClient { PlayerEntity = entity });
                break;
            default:
                _logger.Warning($"Client sent unknown packet type {packet.PacketType}");
                break;
        }
    }

    private void ParseRequest(ref Packet packet, Entity entity)
    {
        var reader = new MessagePackReader(packet.Data);
        var componentId = reader.ReadInt32();
        // TODO
    }
}