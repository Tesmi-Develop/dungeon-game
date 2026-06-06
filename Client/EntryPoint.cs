using Client.InternalSystems;
using Hypercube.Core.Audio.Manager;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.Attributes;
using Hypercube.Core.Execution.Enums;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Resources;
using Hypercube.Core.Resources.Preloading;
using Hypercube.Core.UI.Manager;
using Hypercube.Physics.Collision;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Shared.Data;
using Shared.Helpers;

namespace Client;

public static class EntryPoint
{
    [EntryPoint(EntryPointStage.BeforeInit)]
    public static void Init(DependenciesContainer container)
    {
        NetworkSideContext.NetworkSide = NetworkSide.Client;
        Contacts.Initialize();
        MessagePackHelper.SetupMessagePack();
        container.Register<GameClient>();
    }

    [EntryPoint(EntryPointStage.BeforeEntityInitialization)]
    public static void RenderInit(DependenciesContainer container)
    {
        var resourceManager = container.Resolve<IResourceManager>();
        var loaded = new ManualResetEventSlim(false);
        var preloadContext = resourceManager.CreatePreloadContext();
        var logger = container.Resolve<ILogger>();
       
        container.RegisterSingleton<PreloadContext>(preloadContext);
        container.Register<AnimationContainer>();
        container.Resolve<AnimationContainer>().Initialize();
       
        StartPreload(preloadContext, logger, loaded);
        loaded.Wait();
       
        container.Resolve<AnimationContainer>().Start();
    }
    
    [EntryPoint(EntryPointStage.AfterInit)]
    public static void Start(DependenciesContainer container)
    {
        var logger = container.Resolve<ILogger>();
        var patchManager = container.Resolve<IPatchManager>();
        var world = container.Resolve<IEntitySystemManager>();
        var audio = container.Resolve<IAudioManager>();
        var uiManager = container.Resolve<IUIManager>();
        var resourceManager = container.Resolve<IResourceManager>();
        var gameClient = container.Resolve<GameClient>();
        
        gameClient.Start();
        /*while (!gameClient.Connected)
        {
            gameClient.ConnectAsync("127.0.0.1", 5000).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    logger.Error(task.Exception);
                }
            });
            Thread.Sleep(1000);
        }*/
    }
    
    public static void StartPreload(PreloadContext preloadContext, ILogger logger, ManualResetEventSlim loaded)
    {
        logger.Info("Start load recourses");
        preloadContext.ExecuteAsync(new Progress(logger)).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                logger.Error(task.Exception);
                return;
            }
           
            loaded.Set();
            logger.Info("Finish load recourses");
        });
    }
}

public class Progress : IProgress<PreloadProgress>
{
    private readonly ILogger _logger;
        
    public Progress(ILogger logger)
    {
        _logger = logger;
    }
        
    public void Report(PreloadProgress value)
    {
        if (value.HasError)
        {
            _logger.Error(value.Error!);
            return;
        }
            
        _logger.Info($"Loaded: {value.Loaded}/{value.Total}");
    }
}