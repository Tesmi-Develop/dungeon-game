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
            
            _dependenciesContainer.Register(type);
            priorities.Add((type, attributeData.Priority));
        }
        
        var systems = _dependenciesContainer.ResolveAll();
        return systems
            .OrderBy(e =>
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
        }
    }
    
    public void InvokeInitialize()
    {
        foreach (var system in _systems)
        {
            system.Initialize();
        }
    }
    
    public void InvokePostInitialize()
    {
        foreach (var system in _systems)
        {
            system.PostInitialize();
        }
    }

    public void InvokeBeforeUpdate(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.BeforeUpdate(deltaTime);
        }
    }
    
    public void InvokeUpdate(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.Update(deltaTime);
        }
    }
    
    public void InvokeAfterUpdate(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.AfterUpdate(deltaTime);
        }
    }

    public void InvokeUpdateCycle(float deltaTime)
    {
        InvokeBeforeUpdate(deltaTime);
        InvokeUpdate(deltaTime);
        InvokeAfterUpdate(deltaTime);
    }
}