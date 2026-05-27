using System.Reflection;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Ecs;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Hypercube.Utilities.Helpers;
using Shared.Attributes;
using Shared.SharedSystemRealisation;
using SharedSystem = Shared.SharedSystemRealisation.SharedSystem;
using ClientSystem = Client.Utilities.BaseSystem;

namespace Client.InternalSystems;

public class EntrySystem : EntitySystem
{
    [Dependency] private readonly IDependenciesContainer _globalContainer = null!;
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IPatchManager _patchManager = null!;

    private IDependenciesContainer _dependenciesContainer = null!;
    private List<SharedSystem> _allSystems = null!;
    private List<ClientSystem> _clientSystems = null!;
    private List<IPatch> _patchSystems = null!;
    
    private readonly List<SharedSystem> _beforeInitializeSystems = [];
    private readonly List<SharedSystem> _initializeSystems = [];
    private readonly List<SharedSystem> _afterInitializeSystems = [];
    
    private readonly List<SharedSystem> _beforeGameUpdateSystems = [];
    private readonly List<SharedSystem> _gameUpdateSystems = [];
    private readonly List<SharedSystem> _afterGameUpdateSystems = [];
    
    private readonly List<ClientSystem> _beforeUpdateSystems = [];
    private readonly List<ClientSystem> _updateSystems = [];
    private readonly List<ClientSystem> _afterUpdateSystems = [];

    public long CurrentTick { get; private set; }
    
    public override void Initialize()
    {
        _dependenciesContainer = new DependenciesContainer(_globalContainer);
        _dependenciesContainer.RegisterSingleton<World>(World);
        _allSystems = InstantiateSystems();
        _clientSystems = CollectClientSystems();
        _patchSystems = CollectPatchSystems();
        PreparePhaseSystems();
        RegisterPatchSystems();

        InvokeInitializePhase();
    }
    
    public override void Update(FrameEventArgs args)
    {
        InvokeUpdatePhase(args);
    }
    
    private List<SharedSystem> InstantiateSystems()
    {
        var priorities = new List<(Type Type, int Priority)>();
        var baseSystemType = typeof(SharedSystem);
        
        foreach (var (type, attributeData) in ReflectionHelper.GetAllTypesWithAttribute<EcsSystemAttribute>())
        {
            if (type.IsAbstract || !type.IsAssignableTo(baseSystemType))
            {
                _logger.Warning($"Class {type.Name} does not implement {baseSystemType.Name}");
                continue;
            }
        
            _logger.Trace($"Found system: {type.Name}");
            _dependenciesContainer.Register(type);
            priorities.Add((type, attributeData.Priority));
        }
    
        _dependenciesContainer.ResolveAll();
        
        return priorities
            .OrderByDescending(static p => p.Priority)
            .Select(p => (SharedSystem)_dependenciesContainer.Resolve(p.Type))
            .ToList();
    }

    private List<ClientSystem> CollectClientSystems()
    {
        var list = new List<ClientSystem>();

        foreach (var system in _allSystems)
        {
            if (system is ClientSystem clientSystem)
                list.Add(clientSystem);
        }

        return list;
    }
    
    private List<IPatch> CollectPatchSystems()
    {
        var list = new List<IPatch>();
        
        foreach (var system in _clientSystems)
        {
            if (system is IPatch patchSystem)
                list.Add(patchSystem);
        }

        return list;
    }
    
    private void PreparePhaseSystems()
    {
        _beforeInitializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.BeforeInitialize)));
        _initializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.Initialize)));
        _afterInitializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.AfterInitialize)));

        _beforeGameUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.BeforeGameUpdate)));
        _gameUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.GameUpdate)));
        _afterGameUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.AfterGameUpdate)));
        
        _beforeUpdateSystems.AddRange(SortByMethodPriority(_clientSystems, nameof(ClientSystem.BeforeUpdate)));
        _updateSystems.AddRange(SortByMethodPriority(_clientSystems, nameof(ClientSystem.Update)));
        _afterUpdateSystems.AddRange(SortByMethodPriority(_clientSystems, nameof(ClientSystem.AfterUpdate)));
    }

    private void RegisterPatchSystems()
    {
        foreach (var patchSystem in _patchSystems)
            _patchManager.AddPatch(patchSystem);
    }
    
    private IEnumerable<T> SortByMethodPriority<T>(List<T> systems, string methodName) where T : SharedSystem
    {
        return systems
            .Select(s => new
            {
                System = s,
                Priority = s.GetType()
                    .GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    ?.GetCustomAttribute<PriorityAttribute>()?.Priority ?? 0
            })
            .OrderByDescending(x => x.Priority)
            .Select(x => x.System);
    }

    private void InvokeInitializePhase()
    {
        foreach (var system in _beforeInitializeSystems)
            system.BeforeInitialize();

        foreach (var system in _initializeSystems)
            system.Initialize();
        
        foreach (var system in _afterInitializeSystems)
            system.AfterInitialize();
    }
    
    private void InvokeUpdatePhase(FrameEventArgs eventArgs)
    {
        foreach (var system in _beforeUpdateSystems)
            system.BeforeUpdate(eventArgs);

        foreach (var system in _updateSystems)
            system.Update(eventArgs);
        
        foreach (var system in _afterUpdateSystems)
            system.AfterUpdate(eventArgs);
    }
    
    public void InvokeGameUpdatePhase(long tick, long predictTick)
    {
        CurrentTick = tick;
        
        foreach (var system in _beforeGameUpdateSystems)
            system.BeforeGameUpdate(tick, predictTick);

        foreach (var system in _gameUpdateSystems)
            system.GameUpdate(tick, predictTick);
        
        foreach (var system in _afterGameUpdateSystems)
            system.AfterGameUpdate(tick, predictTick);
    }
}