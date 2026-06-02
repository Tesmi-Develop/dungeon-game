namespace Shared.Attributes;

public class GameStageAttribute : Attribute
{
    public int GameStage { get; }

    public GameStageAttribute(int gameStage)
    {
        GameStage = gameStage;
    }
}