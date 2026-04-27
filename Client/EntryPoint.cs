using Hypercube.Core.Audio.Manager;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.Attributes;
using Hypercube.Core.Execution.Enums;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Resources;
using Hypercube.Core.UI;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Shared.Helpers;

namespace Client;

public static class EntryPoint
{
    [EntryPoint(EntryPointStage.BeforeInit)]
    public static void Init(DependenciesContainer container)
    {
        MessagePackHelper.SetupMessagePack();
        container.Register<GameClient>();
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
        
        /*var sound = resourceManager.Load<Audio>("/audio/game_boi_3.wav");
        var source = audio.CreateSource(sound);
        
        source.Start();*/
        
        gameClient.Start();
        gameClient.ConnectAsync("127.0.0.1", 5000).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                logger.Error(task.Exception);
            }
        });
    }
}