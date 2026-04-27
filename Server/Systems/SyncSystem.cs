using System.Buffers;
using Arch.Core;
using Server.Extensions;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using MessagePack;
using Server.Components;
using Server.Components.Events;
using Server.Events;
using Server.Helpers;
using Shared.Data;
using Shared.NetworkUtilities;

namespace Server.Systems;

[EcsSystem(EcsPriority.Low)]
public class SyncSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IEventBus _eventBus = null!;
    
    private readonly QueryDescription _query = new QueryDescription().WithAll<Dirty>();
    private readonly QueryDescription _clientQuery = new QueryDescription().WithAll<ClientData>();
    private readonly QueryDescription _syncQuery = new QueryDescription().WithAll<NetworkEntityTag>();
    private readonly ArrayBufferWriter<byte> _bufferWriter = new(1024); 
    private readonly List<(Entity entity, int NetId)> _removalQueue = [];
    private readonly List<(Entity entity, int NetId)> _additionalQueue = [];
    private readonly HashSet<long> _destroyedEntities = [];
    private readonly List<Entity> _tempEntityList = [];
    
    private void RegisterRemovalHook<T>(int netId) where T : struct
    {
        world.SubscribeComponentRemoved<T>(Handler);
        return;

        void Handler(in Entity entity, ref T comp) 
        {
            _removalQueue.Add((entity, netId));
        }
    }
    
    private void RegisterAddedHook<T>(int netId) where T : struct
    {
        world.SubscribeComponentAdded<T>(Handler);
        return;

        void Handler(in Entity entity, ref T comp) 
        {
            world.Add<NetworkEntityTag>(entity);
            _additionalQueue.Add((entity, netId));
        }
    }

    public override void Initialize()
    {
        var networkComponents = NetworkHelper.GetNetworkComponentMetadata(world);

        foreach (var (netId, type) in networkComponents.ComponentsById)
        {
            var method = GetType().GetMethod(nameof(RegisterRemovalHook), 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        
            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(this, [netId]);
            
            method = GetType().GetMethod(nameof(RegisterAddedHook), 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        
            genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(this, [netId]);
        }
        
        world.SubscribeComponentRemoved((in Entity entity, ref NetworkEntityTag _) =>
        {
            _destroyedEntities.Add(entity.GetFullMask());
        });
        
        _eventBus.Subscribe((Entity _, ref ClientData playerData, ref NewEntityClient _) =>
        {
            SendFullStateToClient(ref playerData);
        });
    }

    public void SendFullStateToClient(ref ClientData clientData)
    {
        var networkComponents = NetworkHelper.GetNetworkComponentMetadata(world);
        _bufferWriter.Clear();

        var counts = world.CountEntities(in _syncQuery);
        MessagePackSerializer.Serialize(_bufferWriter, counts);
        
        world.Query(in _syncQuery, entity =>
        {
            MessagePackSerializer.Serialize(_bufferWriter, entity.GetFullMask());
            
            var validComponents = 0;
            foreach (var (_, type) in networkComponents.ComponentsById)
                if (world.Has(entity, type)) validComponents++;

            MessagePackSerializer.Serialize(_bufferWriter, validComponents);
            
            foreach (var (netId, type) in networkComponents.ComponentsById)
            {
                if (!world.Has(entity, type)) 
                    continue;
                
                var componentData = world.Get(entity, type);
                MessagePackSerializer.Serialize(_bufferWriter, netId);
                ((ISynced)componentData!).Serialize(_bufferWriter, null);
            }
        });
        
        var packet = new Packet
        {
            PacketType = PacketType.Hydrate,
            Data = _bufferWriter.WrittenSpan.ToArray()
        };
        
        clientData.PendingPackets.Enqueue(packet);
    }

    public override void Update(float deltaTime)
    {
        _bufferWriter.Clear();
        
        SerializeDestroyedEntities();
        BroadcastSyncPacket(PacketType.EntitiesDeletion, DeliveryMethod.ReliableOrdered);
        _bufferWriter.Clear();
        
        SerializeRemovedComponents();
        BroadcastSyncPacket(PacketType.ComponentsDeletion, DeliveryMethod.ReliableOrdered);
        _bufferWriter.Clear();
        
        SerializeAddedComponents();
        BroadcastSyncPacket(PacketType.ComponentsAddition, DeliveryMethod.ReliableOrdered);
        _bufferWriter.Clear();
        
        SerializeDirtyChanges();
        BroadcastSyncPacket(PacketType.Dirty, DeliveryMethod.ReliableOrdered);
        _bufferWriter.Clear();
    }

    private void SerializeDestroyedEntities()
    {
        if (_destroyedEntities.Count <= 0)
            return;
        
        MessagePackSerializer.Serialize(_bufferWriter, _destroyedEntities.Count);
        foreach (var mask in _destroyedEntities)
            MessagePackSerializer.Serialize(_bufferWriter, mask);
        
        _destroyedEntities.Clear();
    }

    private void SerializeRemovedComponents()
    {
        var actualCount = _removalQueue.Count(r => world.IsAlive(r.entity));
        if (actualCount <= 0)
            return;
        
        MessagePackSerializer.Serialize(_bufferWriter, actualCount);
        foreach (var (entity, netId) in _removalQueue)
        {
            if (!world.IsAlive(entity)) 
                continue;
            
            MessagePackSerializer.Serialize(_bufferWriter, entity.GetFullMask());
            MessagePackSerializer.Serialize(_bufferWriter, netId);
        }
        
        _removalQueue.Clear();
    }
    
    private void SerializeAddedComponents()
    {
        var actualCount = _additionalQueue.Count(r => world.IsAlive(r.entity));
        if (actualCount <= 0)
            return;
        
        MessagePackSerializer.Serialize(_bufferWriter, actualCount);
        
        foreach (var (entity, netId) in _additionalQueue)
        {
            if (!world.IsAlive(entity)) 
                continue;
            
            MessagePackSerializer.Serialize(_bufferWriter, entity.GetFullMask());
            MessagePackSerializer.Serialize(_bufferWriter, netId);
            
            var componentType = NetworkHelper.GetNetworkComponentById(world, netId);
            var synced = (ISynced)world.Get(entity, componentType)!;
            synced.Serialize(_bufferWriter, null);
        }
        
        _additionalQueue.Clear();
    }

    private void SerializeDirtyChanges()
    {
        var dirtyEntitiesCount = world.CountEntities(in _query);
        MessagePackSerializer.Serialize(_bufferWriter, dirtyEntitiesCount);

        _tempEntityList.Clear(); 

        world.Query(in _query, (Entity entity, ref Dirty dirty) =>
        {
            _tempEntityList.Add(entity);
            MessagePackSerializer.Serialize(_bufferWriter, entity.GetFullMask());

            if (dirty.ComponentIds.Count == 0)
            {
                MessagePackSerializer.Serialize(_bufferWriter, 0);
                _logger.Warning($"Entity {entity} is Dirty but has no component IDs.");
                return;
            }

            MessagePackSerializer.Serialize(_bufferWriter, dirty.ComponentIds.Count);
            foreach (var id in dirty.ComponentIds)
            {
                MessagePackSerializer.Serialize(_bufferWriter, id);
                var componentType = NetworkHelper.GetNetworkComponentById(world, id);
                var synced = (ISynced)world.Get(entity, componentType)!;
                synced.Serialize(_bufferWriter, null);
            }
        });

        foreach(var e in _tempEntityList) 
            world.Remove<Dirty>(e);
    }

    private void BroadcastSyncPacket(PacketType packetType, DeliveryMethod deliveryType)
    {
        if (_bufferWriter.WrittenCount <= 3) 
            return;

        var packet = new Packet
        {
            PacketType = packetType,
            DeliveryType = deliveryType,
            Data = _bufferWriter.WrittenSpan.ToArray() 
        };

        PushAllClientsSyncData(packet);
    }

    private void PushAllClientsSyncData(Packet packet)
    {
        world.Query(in _clientQuery, (Entity _, ref ClientData payload) =>
        {
            payload.PendingPackets.Enqueue(packet);
        });
    }
}