using Client.UI.MainMenu;
using Hypercube.Core.Ecs;
using Hypercube.Core.UI.Elements;
using Hypercube.Core.UI.Manager;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Dimensions;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;

namespace Client.InternalSystems;

public class TestMainMenuSystem : EntitySystem
{
    [Dependency] private IUIManager _uiManager = null!;

    public override void Initialize()
    {
        var entry = new MainMenuEntry(_uiManager);
        entry.Start();
    }
}