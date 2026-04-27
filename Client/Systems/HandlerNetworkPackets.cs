using Client.Events;
using Hypercube.Core.Ecs;
using Hypercube.Utilities.Dependencies;
using Shared.Data;

namespace Client.Systems;

public class HandlerNetworkPackets : EntitySystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    private const int MaxPacketPerUpdate = 60;
    private List<Packet> _packetBuffer = new(128);

    public override void Update(float deltaTime)
    {
        var counter = 0;

        while (_packetBuffer.Count > 0 && counter < MaxPacketPerUpdate && counter < _packetBuffer.Count)
        {
            var packet = _packetBuffer[counter];
            var handled = HandlePacket(packet, false);

            if (handled)
                _packetBuffer.RemoveAt(counter);
            
            counter++;
        }
        
        while (_gameClient.Packets.TryDequeue(out var packet) && counter < MaxPacketPerUpdate)
        {
            counter++;
            HandlePacket(packet, true);
        }
    }

    private bool HandlePacket(Packet packet, bool buffering)
    {
        var eventData = new HandlePacketEvent { Packet = packet };
        Raise(ref eventData);

        if (eventData.Handled)
            return true;
        
        if (buffering)
            _packetBuffer.Add(packet);
        
        return false;
    }
}