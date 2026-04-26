using Hypercube.Core.Audio.Manager;
using Hypercube.Core.Audio.Resources;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.Attributes;
using Hypercube.Core.Execution.Enums;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Core.Systems.Transform;
using Hypercube.Core.UI;
using Hypercube.Core.UI.Elements;
using Hypercube.Core.Viewports;
using Hypercube.Core.Windowing.Manager;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;

namespace Rainclipse;

public static class EntryPoint
{
    [EntryPoint(EntryPointStage.BeforeInit)]
    public static void Init(DependenciesContainer container)
    {
        // Init
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
            
        var patch = new TestPatch();

        container.Inject(patch);
        patchManager.AddPatch(patch);
        
        var entity = world.Create();
        world.Add<TestComponent>(entity);
        world.Add<TransformComponent>(entity);
        world.Add(entity, new SpriteComponent { Path = "/textures/default.png" });
        
        var sound = resourceManager.Load<Audio>("/audio/game_boi_3.wav");
        var source = audio.CreateSource(sound);
    }
}