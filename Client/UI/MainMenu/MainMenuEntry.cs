using Hypercube.Core.UI.Elements;
using Hypercube.Core.UI.Manager;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Dimensions;
using Hypercube.Mathematics.Vectors;

namespace Client.UI.MainMenu;

public class MainMenuEntry
{
    private readonly IUIManager _uiManager;

    public MainMenuEntry(IUIManager uiManager)
    {
        _uiManager = uiManager;
    }

    public void Start()
    {
        var root = _uiManager.Root.AddChild(new Rectangle
        {
            AnchorPoint = new Vector2(1, 0.5f),
            Position = new HDim2(1, 0, 0.5f, 0),
            Size = new HDim2(0.2f, 0, 1, 0),
            Color = new Color("#77e36d55"),
        });
    }
}