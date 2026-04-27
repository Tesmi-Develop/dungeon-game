using System.Diagnostics;
using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Events;
using Shared.Helpers;

namespace Server;

public static class Program
{
    public static void Main()
    {
        MessagePackHelper.SetupMessagePack();
        var logger = new ConsoleLogger();
        var world = World.Create();
        var dependenciesContainer = new DependenciesContainer();
        var eventBus = new EventBus();
        
        dependenciesContainer.RegisterSingleton<IEventBus>(eventBus);
        var systemHandler = new EcsSystemHandler(world, logger, dependenciesContainer);
        
        systemHandler.InvokePreInitialize();
        systemHandler.InvokeInitialize();
        systemHandler.InvokePostInitialize();

        var tickRate = 30d;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        var previous = stopWatch.Elapsed.TotalMilliseconds;
        
        while (true)
        {
            var currentTime = stopWatch.Elapsed.TotalMilliseconds;
            var delta = currentTime - previous;
            
            if (delta >= 1000d / tickRate)
            {
                systemHandler.InvokeUpdateCycle((float)delta);
                previous = currentTime;
                continue;
            }

            Thread.Yield();
        }
    }
}