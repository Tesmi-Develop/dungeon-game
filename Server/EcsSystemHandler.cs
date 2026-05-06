using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Hypercube.Utilities.Helpers;
using Server.Utilities;

namespace Server;

public class EcsSystemHandler
{
    private readonly List<BaseSystem> _systems;
    private readonly IDependenciesContainer _dependenciesContainer;
    private readonly ILogger _logger;

    public EcsSystemHandler(World world, ILogger logger, IDependenciesContainer dependenciesContainer)
    {
        _logger = logger;
        _dependenciesContainer = dependenciesContainer;
        _dependenciesContainer.RegisterSingleton<World>(world); 
        PrepareLogger();
        _systems = InstantiateSystems();
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

    private List<BaseSystem> InstantiateSystems()
    {
        var priorities = new List<(Type, int)>();
        
        foreach (var (type, attributeData) in ReflectionHelper.GetAllTypesWithAttribute<EcsSystemAttribute>())
        {
            var baseSystemType = typeof(BaseSystem);
            
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
        
        var systems = new List<BaseSystem>();

        foreach (var (type, _) in priorities)
            systems.Add((BaseSystem)_dependenciesContainer.Resolve(type));
        
        return systems
            .OrderByDescending(e =>
            {
                var type = e.GetType();
                return priorities.Find(r => r.Item1 == type).Item2;
            })
            .OfType<BaseSystem>()
            .ToList();
    }

    public void InvokePreInitialize()
    {
        foreach (var system in _systems)
        {
            system.PreInitialize();
            _logger.Trace($"Pre initialized system: {system.GetType().Name}");
        }
    }
    
    public void InvokeInitialize()
    {
        foreach (var system in _systems)
        {
            system.Initialize();
            _logger.Trace($"Initialized system: {system.GetType().Name}");
        }
    }
    
    public void InvokePostInitialize()
    {
        foreach (var system in _systems)
        {
            system.PostInitialize();
            _logger.Trace($"Post initialized system: {system.GetType().Name}");
        }
    }

    public void InvokeBeforeUpdate(long tick)
    {
        foreach (var system in _systems)
        {
            system.BeforeUpdate(tick);
        }
    }
    
    public void InvokeUpdate(long tick)
    {
        foreach (var system in _systems)
        {
            system.Update(tick);
        }
    }
    
    public void InvokeAfterUpdate(long tick)
    {
        foreach (var system in _systems)
        {
            system.AfterUpdate(tick);
        }
    }

    public void InvokeUpdateCycle(long tick)
    {
        InvokeBeforeUpdate(tick);
        InvokeUpdate(tick);
        InvokeAfterUpdate(tick);
    }
}