using System.Reflection;
using Client.Data;
using Hypercube.Core.Ecs;
using Hypercube.Utilities.Dependencies;

namespace Client.InternalSystems;

public class SceneSystem : EntitySystem
{
    [Dependency] private readonly EntrySystem _entrySystem = null!;
    public SceneType SceneType = SceneType.Menu;

    public override void Initialize()
    {
        SetScene(SceneType);
    }

    public void SetScene(SceneType sceneType)
    {
        Logger.Debug($"Setting scene to {sceneType}");
        SceneType = sceneType;
        _entrySystem.StartSystems((systemType) =>
        {
            var data = systemType.GetCustomAttribute<SceneAttribute>();
            if (data is null)
                return true;
            
            return data.SceneType == sceneType;
        });
    }
}