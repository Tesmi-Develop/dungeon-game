using System.Diagnostics;
using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using MessagePack;
using MessagePack.Resolvers;
using Server.Events;
using Server.MessagePackExtensions;

namespace Server;

public static class Program
{
    private static void SetupMessagePack()
    {
        StaticCompositeResolver.Instance.Register(
            CustomResolver.Instance,
            MessagePack.Resolvers.StandardResolver.Instance
        );
        
        var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
        MessagePackSerializer.DefaultOptions = options;
    }
    
    public static void Main()
    {
        SetupMessagePack();
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