using System.Reflection;
using Hypercube.Ecs;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Hypercube.Utilities.Helpers;
using Server.Utilities;
using Shared.Attributes;
using Shared.SharedSystemRealisation;

namespace Server;

public class EcsSystemEntry
{
    private readonly List<SharedSystem> _allSystems;
    private readonly IDependenciesContainer _dependenciesContainer;
    private readonly ILogger _logger;
    
    private readonly List<SharedSystem> _beforeInitializeSystems = [];
    private readonly List<SharedSystem> _initializeSystems = [];
    private readonly List<SharedSystem> _afterInitializeSystems = [];
    
    private readonly List<SharedSystem> _beforeUpdateSystems = [];
    private readonly List<SharedSystem> _updateSystems = [];
    private readonly List<SharedSystem> _afterUpdateSystems = [];
    
    public EcsSystemEntry(World world, ILogger logger, IDependenciesContainer dependenciesContainer)
    {
        _logger = logger;
        _dependenciesContainer = dependenciesContainer;
        _dependenciesContainer.RegisterSingleton<World>(world);
        PrepareLogger();
        
        _allSystems = InstantiateSystems();
        PreparePhaseSystems();
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

    private void PreparePhaseSystems()
    {
        _beforeInitializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.BeforeInitialize)));
        _initializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.Initialize)));
        _afterInitializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.AfterInitialize)));

        _beforeUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.BeforeGameUpdate)));
        _updateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.GameUpdate)));
        _afterUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(SharedSystem.AfterGameUpdate)));
    }

    private IEnumerable<SharedSystem> SortByMethodPriority(List<SharedSystem> systems, string methodName)
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
    
    public void InvokeBeforeInitialize() => InvokePhase(_beforeInitializeSystems, static s => s.BeforeInitialize(), "PreInitialize");
    public void InvokeInitialize() => InvokePhase(_initializeSystems, static s => s.Initialize(), "Initialize");
    public void InvokeAfterInitialize() => InvokePhase(_afterInitializeSystems, static s => s.AfterInitialize(), "PostInitialize");

    public void InvokeBeforeUpdate(long tick)
    {
        var count = _beforeUpdateSystems.Count;
        for (var i = 0; i < count; i++)
        {
            _beforeUpdateSystems[i].BeforeGameUpdate(tick, tick);
        }
    }

    public void InvokeUpdate(long tick)
    {
        var count = _updateSystems.Count;
        for (var i = 0; i < count; i++)
        {
            _updateSystems[i].GameUpdate(tick, tick);
        }
    }

    public void InvokeAfterUpdate(long tick)
    {
        var count = _afterUpdateSystems.Count;
        for (var i = 0; i < count; i++)
        {
            _afterUpdateSystems[i].AfterGameUpdate(tick, tick);
        }
    }

    private void InvokePhase(List<SharedSystem> systems, Action<SharedSystem> action, string phaseName)
    {
        foreach (var system in systems)
        {
            action(system);
            _logger.Trace($"{phaseName} system: {system.GetType().Name}");
        }
    }
    
    public void InvokeInitializePhase()
    {
        InvokeBeforeInitialize();
        InvokeInitialize();
        InvokeAfterInitialize();
    }

    public void InvokeGameUpdatePhase(long tick)
    {
        InvokeBeforeUpdate(tick);
        InvokeUpdate(tick);
        InvokeAfterUpdate(tick);
    }
    
    private void PrepareLogger()
    {
        _dependenciesContainer.Register<ILogger>((_, target) =>
        {
            if (target is null)
                return new ConsoleLogger();
            
            return new MyLogger(target);
        }, DependencyLifetime.Transient);
        
        _dependenciesContainer.Register<Logger>((_, target) =>
        {
            if (target is null)
                return new ConsoleLogger();
            
            return new MyLogger(target);
        }, DependencyLifetime.Transient);
    }
}