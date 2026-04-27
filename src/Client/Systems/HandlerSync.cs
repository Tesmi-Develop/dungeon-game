using Client.Events;
using Hypercube.Core.Ecs;
using Hypercube.Ecs;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using MessagePack;
using Shared.Data;

namespace Client.Systems;

public class HandlerSync : EntitySystem
{
    [Dependency] private readonly ILogger _logger = null!;
    private readonly Dictionary<long, Entity> _networkEntitiesById = [];
    private readonly Dictionary<Entity, long> _networkEntitiesByEntity = [];
    
    public override void Initialize()
    {
        Subscribe<HandlePacketEvent>(OnHandlePacket);
    }

    private void OnHandlePacket(ref HandlePacketEvent args)
    {
        switch (args.Packet.PacketType)
        {
            case PacketType.Hydrate:
                try
                {
                    DoHydrate(args.Packet.Data);
                }
                catch (Exception e)
                {
                    var decimalData = string.Join(", ", args.Packet.Data.ToArray());

                    _logger.Error($"Hydrate Error: {e.Message}");
                    _logger.Error($"Packet Length: {args.Packet.Data.Length} bytes");
                    _logger.Error($"Data (dec): [{decimalData}]");
                }
                break;
            case PacketType.Dirty:
                DoDirty(args.Packet.Data);
                break;
            case PacketType.EntitiesDeletion:
                DoEntitiesDeletion(args.Packet.Data);
                break;
            case PacketType.ComponentsDeletion:
                DoComponentDeletion(args.Packet.Data);
                break;
            case PacketType.ComponentsAddition:
                DoComponentAddition(args.Packet.Data);
                break;
            default:
                return;
        }
        
        args.Handled = true;
    }
    
    private void DoHydrate(ReadOnlyMemory<byte> payload)
    {
        var reader = new MessagePackReader(payload);
        var countEntities = reader.ReadInt32();

        for (var i = 0; i < countEntities; i++)
        {
            var entityMask = reader.ReadInt64();
            var countComponents = reader.ReadInt32();

            if (countComponents <= 0)
                continue;

            var entity = GetNetworkEntity(entityMask);
            for (var j = 0; j < countComponents; j++)
            {
                var componentId = reader.ReadInt32();
                var componentData = payload[(int)reader.Consumed..];
                NetworkFactory.AddComponentFromPayload(componentId, entity, World, componentData);
                reader.Skip();
            }
        }
    }

    private void DoDirty(ReadOnlyMemory<byte> payload)
    {
        var reader = new MessagePackReader(payload);
        var entityCounts = reader.ReadInt32();

        for (var i = 0; i < entityCounts; i++)
        {
            var entityId = reader.ReadInt64();
            if (!HasNetworkEntity(entityId))
            {
                // TODO do packet splitting and don't drop the current one
                continue;
            }

            var entity = GetNetworkEntity(entityId);
            var componentCounts = reader.ReadInt32();

            for (var j = 0; j < componentCounts; j++)
            {
                var componentId = reader.ReadInt32();
                var componentData = payload[(int)reader.Consumed..];
                
                NetworkFactory.PatchComponentFromPayload(componentId, entity, World, componentData);
                reader.Skip();
            }
        }
    }

    private void DoComponentDeletion(ReadOnlyMemory<byte> payload)
    {
        var reader = new MessagePackReader(payload);
        var entityCount = reader.ReadInt32();

        for (var i = 0; i < entityCount; i++)
        {
            var entityId = reader.ReadInt64();
            var entity = GetNetworkEntity(entityId);
            var componentId = reader.ReadInt32();
            NetworkFactory.RemoveComponent(componentId, entity, World);
        }
    }

    private void DoComponentAddition(ReadOnlyMemory<byte> payload)
    {
        var reader = new MessagePackReader(payload);
        var entityCount = reader.ReadInt32();

        for (var i = 0; i < entityCount; i++)
        {
            var entityId = reader.ReadInt64();
            var entity = GetNetworkEntity(entityId);
            var componentId = reader.ReadInt32();
            var componentData = payload[(int)reader.Consumed..];
            
            NetworkFactory.AddComponentFromPayload(componentId, entity, World, componentData);
            reader.Skip();
        }
    }

    private void DoEntitiesDeletion(ReadOnlyMemory<byte> payload)
    {
        var reader = new MessagePackReader(payload);
        var count = reader.ReadInt32();

        for (var i = 0; i < count; i++)
        {
            var entityId = reader.ReadInt64();
            RemoveNetworkEntity(entityId);
        }
    }

    public Entity GetNetworkEntity(long entityId)
    {
        if (_networkEntitiesById.TryGetValue(entityId, out var entity))
            return entity;
        
        entity = EntityCreate();
        _networkEntitiesById.Add(entityId, entity);
        _networkEntitiesByEntity.Add(entity, entityId);
        
        return  entity;
    }

    public bool TryGetNetworkEntityMask(Entity entity, out long entityMask)
    {
        entityMask = 0;
        
        if (!_networkEntitiesByEntity.TryGetValue(entity, out var entityId)) 
            return false;
        
        entityMask = entityId;
        return true;

    }

    public bool HasNetworkEntity(long entityId)
    {
        return _networkEntitiesById.ContainsKey(entityId);
    }

    public void RemoveNetworkEntity(long entityId)
    {
        if (!_networkEntitiesById.Remove(entityId, out var entity))
            return;
        
        _networkEntitiesByEntity.Remove(entity);
        World.Delete(entity);
    }
}