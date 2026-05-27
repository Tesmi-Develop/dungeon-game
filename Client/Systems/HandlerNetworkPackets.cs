using Client.Events;
using Client.Utilities;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Utilities.Dependencies;
using Shared.Attributes;
using Shared.Data;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class HandlerNetworkPackets : BaseSystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    private const int MaxPacketPerUpdate = 60;
    private List<Packet> _packetBuffer = new(128);
    
    [Priority(EcsPriority.High)]
    public override void BeforeUpdate(FrameEventArgs deltaTime)
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