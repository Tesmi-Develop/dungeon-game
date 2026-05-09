using System.Buffers;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using MessagePack;
using Shared.Data;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class NetworkHelper : BaseSystem
{
    [Dependency] private readonly GameClient _client = null!;
    [Dependency] private readonly PredictSystem  _predictSystem = null!;
    private readonly ArrayBufferWriter<byte> _buffer = new(1025);
    
    public void SendRequest<T>(T request, DeliveryMethod deliveryMethod, long tick = -1)
    {
        _buffer.Clear();
        
        var id = NumeratorGenerator.GetId(typeof(T));
        var writer = new MessagePackWriter(_buffer);
        
        writer.WriteInt32(id);
        writer.WriteInt64(tick);
        NetworkFactory.SerializeRequestComponent(id, ref writer, request);
        writer.Flush();
        
        _client.Send(PacketType.Request, _buffer.WrittenMemory.ToArray(), deliveryMethod);
    }

    public void SendInputIfPredicting<T>(T request, DeliveryMethod deliveryMethod)
    {
        if (_predictSystem.IsRollback)
            return;
        
        SendRequest(request, deliveryMethod, _predictSystem.PredictTick);
    }
}