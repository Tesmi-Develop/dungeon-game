using Client.Data;

namespace Client;

public class SceneAttribute : Attribute
{
    public SceneType SceneType;
    
    public SceneAttribute(SceneType sceneType)
    {
        SceneType = sceneType;
    }
}