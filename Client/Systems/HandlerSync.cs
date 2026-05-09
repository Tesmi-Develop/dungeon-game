using Client.Events;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Ecs;
using Hypercube.Utilities.Dependencies;
using MessagePack;
using Shared.Data;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class HandlerSync : BaseSystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly PredictSystem _predictSystem = null!;
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
                DoHydrate(args.Packet.Data);
                break;
            case PacketType.Dirty:
                DoDirty(args.Packet.Data, args.Packet.Tick);
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
        Logger.Trace("Start hydrate");
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
                NetworkFactory.AddComponentFromPayload(componentId, entity, World, ref reader);
                Logger.Trace($"Added component with type {NumeratorGenerator.GetType(componentId).Name}, entity {entity} with ServerMask: {entityMask}");
            }
        }
    }

    private void DoDirty(ReadOnlyMemory<byte> payload, long packetServerTick)
    {
        var reader = new MessagePackReader(payload);
        var entityCounts = reader.ReadInt32();

        for (var i = 0; i < entityCounts; i++)
        {
            var skipDirty = false;
            var entityId = reader.ReadInt64();
            
            if (!HasNetworkEntity(entityId))
            {
                skipDirty = true;
                // TODO do packet splitting and don't drop the current one
            }

            var entity = GetNetworkEntity(entityId);
            var componentCounts = reader.ReadInt32();

            for (var j = 0; j < componentCounts; j++)
            {
                var componentId = reader.ReadInt32();

                if (!skipDirty)
                {
                    NetworkFactory.PatchComponentFromPayload(componentId, entity, World, packetServerTick, ref reader);
                    _predictSystem.ReconcileState(entity);
                }
                else
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
        Logger.Trace("Start component addition");
        var reader = new MessagePackReader(payload);
        var entityCount = reader.ReadInt32();

        for (var i = 0; i < entityCount; i++)
        {
            var entityId = reader.ReadInt64();
            var entity = GetNetworkEntity(entityId);
            var componentId = reader.ReadInt32();
            
            NetworkFactory.AddComponentFromPayload(componentId, entity, World, ref reader);
            Logger.Trace($"Added component {NumeratorGenerator.GetType(componentId).Name}, entity {entity} with ServerMask: {entityId}");
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
        
        Logger.Trace($"Created Network Entity {entity} with ServerMask: {entityId}");
        
        return  entity;
    }

    public bool TryGetNetworkEntityMask(Entity entity, out long entityMask)
    {
        entityMask = -1;
        
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