using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using MessagePack;
using Server.Components;
using Server.Utilities;
using Shared.Attributes;
using Shared.Data;
using Shared.SharedSystemRealisation;

namespace Server.Systems.Network;

[EcsSystem(EcsPriority.High)]
public class HandleIncomingClientPacketsSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    private Query _query = null!;
    
    public override void Initialize()
    {
        _query = GetQuery().WithAll<ClientData>().Build();
    }
    
    public override void BeforeGameUpdate(long tick, long _)
    {
        _query.With((Entity entity, ref ClientData playerData) =>
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

    [Priority(EcsPriority.Low)]
    public override void AfterGameUpdate(long tick, long predictTick)
    {
        _query.With((Entity entity, ref ClientData clientData) =>
        {
            var inputs = clientData.InputsWithTick[tick % clientData.InputsWithTick.Length];
            inputs.Clear();
        });
    }

    private void ProcessPacket(ref Packet packet, Entity entity)
    {
        switch (packet.PacketType)
        {
            case PacketType.Request:
                ParseRequest(ref packet, entity);
                break;
            default:
                _logger.Warning($"Client sent unknown packet type {packet.PacketType}");
                break;
        }
    }

    private void ParseRequest(ref Packet packet, Entity clientEntity)
    {
        var reader = new MessagePackReader(packet.Data);
        var componentId = reader.ReadInt32();
        var tick = reader.ReadInt64();

        ref var clientData = ref World.Get<ClientData>(clientEntity);
        var data = NetworkFactory.DeserializeRequestComponent(componentId, ref reader);
        
        if (tick == -1)
        {
            clientData.Inputs.Add(new InputData { Tick = -1, Input = data });
            return;
        }

        var inputs = clientData.InputsWithTick[tick % clientData.InputsWithTick.Length];
        inputs[data.GetType()] = new InputData { Tick = tick, Input = data };
    }
}