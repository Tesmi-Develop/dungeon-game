using Hypercube.Core.Resources;
using Hypercube.Ecs;
using Hypercube.Ecs.Events;
using Hypercube.Physics.Collision;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Utilities;
using Shared;
using Shared.Data;
using Shared.Helpers;

namespace Server;

public static class Program
{
    private static readonly int MaxCatchUpTicks = 5;

    private static void InitResourceManager(DependenciesContainer dependenciesContainer)
    {
        var instance = new ResourceManager();
        dependenciesContainer.RegisterSingleton<IResourceManager>(instance);
        instance.AddAllLoaders();
        instance.Mount(Hypercube.Core.Config.MountFolders);
    }
    
    public static void Main()
    {
        NetworkSideContext.NetworkSide = NetworkSide.Server;
        Thread.CurrentThread.Name = "Main";
        var dependenciesContainer = new DependenciesContainer();
        
        Contacts.Initialize();
        MessagePackHelper.SetupMessagePack();
        InitResourceManager(dependenciesContainer);
        
        var logger = new ConsoleLogger();
        var world = new World();
        var time = new Time();

        world.WarmUpComponentsAsync();
        dependenciesContainer.RegisterSingleton<IEventBus>(world.Events);
        dependenciesContainer.RegisterSingleton<Time>(time);
        
        var systemEntry = new EcsSystemEntry(world, logger, dependenciesContainer);
        
        systemEntry.InvokeInitializePhase();

        time.Stopwatch.Start();
        StartCycle(time, systemEntry);
    }

    public static void StartCycle(Time time, EcsSystemEntry entry)
    {
        var tickIntervalMs = 1000.0 / Config.TickRate;
        var nextTickTime = time.Stopwatch.Elapsed.TotalMilliseconds;
        var startTime = time.Stopwatch.Elapsed.TotalMilliseconds;

        while (true)
        {
            var currentTime = time.Stopwatch.Elapsed.TotalMilliseconds;
            var targetTick = (int)Math.Floor((currentTime - startTime) / tickIntervalMs);
            var ticksToProcess = targetTick - time.Tick;

            if (ticksToProcess > 0)
            {
                var ticksToSimulate = Math.Min(ticksToProcess, MaxCatchUpTicks);
                
                if (ticksToProcess > MaxCatchUpTicks)
                {
                    var skipped = ticksToProcess - MaxCatchUpTicks;
                    time.Tick += skipped;
                }
                
                for (var i = 0; i < ticksToSimulate; i++)
                {
                    entry.InvokeGameUpdatePhase(time.Tick);
                    time.Tick++;
                }
            }

            Thread.Sleep(1);
        }
    }
}