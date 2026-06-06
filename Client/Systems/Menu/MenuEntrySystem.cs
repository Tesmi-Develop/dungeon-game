using Client.Data;
using Client.InternalSystems;
using Client.Utilities;
using Hypercube.Core.Graphics.Patching;
using Hypercube.Core.Graphics.Rendering;
using Hypercube.Core.Graphics.Rendering.Context;
using Hypercube.Core.UI;
using Hypercube.Core.UI.Elements;
using Hypercube.Core.UI.Elements.Buttons;
using Hypercube.Core.UI.Elements.Containers;
using Hypercube.Core.UI.Manager;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Dimensions;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.SharedSystemRealisation;

namespace Client.Systems.Menu;

[EcsSystem, Scene(SceneType.Menu)]
public class MenuEntrySystem : BaseSystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly SceneSystem _sceneSystem = null!;
    [Dependency] private readonly IUIManager _uiManager = null!;
    
    private Element? _parentElement;

    public override void Destroy()
    {
       _parentElement?.Parent?.RemoveChild(_parentElement);
    }

    public override void Initialize()
    {
        var buttonContainer = _uiManager.Root.AddChild(new LayoutContainer()
        {
            Orientation = Orientation.Vertical,
            Position = HDim2.ScalarHalf,
            Size = new HDim2(0.3f, 0, 1f, 0),
            AnchorPoint = new Vector2(0.5, 1),
            VAlignment = Alignment.End,
            Direction = Direction.Backward,
            Spacing = new HDim(0, 16),
            Padding = HDimRect.All(new HDim(0, 0)),
        });

        var playLocal = buttonContainer.AddChild(new ButonLabel()
        {
            Size = new HDim2(1.0f, 0, 0, 50),
            
        });
        playLocal.Fill.Color = Color.White;
        playLocal.Label.FontColor = Color.Black;
        playLocal.Label.Text = "Подключиться по локальной сети";
        playLocal.OnClicked += () => WrapConnectInLogger(ConnectLocal);
        
        var playGlobal = buttonContainer.AddChild(new ButonLabel()
        {
            Size = new HDim2(1.0f, 0, 0, 50),
            
        });
        playGlobal.Fill.Color = Color.White;
        playGlobal.Label.FontColor = Color.Black;
        playGlobal.Label.Text = "Подключиться к глобальному серверу";
        playGlobal.OnClicked += () => WrapConnectInLogger(ConnectGlobal);

        _parentElement = buttonContainer;
    }

    private void WrapConnectInLogger(Func<Task> connect)
    {
        connect().ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                Logger.Error(task.Exception);
            }
        });
    }

    private async Task StartConnect(string address, int port, int timeoutMs = 10000)
    {
        var startTime = DateTime.UtcNow;
    
        while (!_gameClient.Connected)
        {
            if ((DateTime.UtcNow - startTime).TotalMilliseconds > timeoutMs)
                throw new TimeoutException($"Не удалось подключиться к {address}:{port} в течение {timeoutMs} мс");
            
            await _gameClient.ConnectAsync(address, port);
            await Task.Delay(1000);
        }
    }
    
    private async Task ConnectLocal()
    {
        if (_gameClient.Connected)
            return;

        await StartConnect("127.0.0.1", 5000);
        _sceneSystem.SetScene(SceneType.Game);
    }

    private async Task ConnectGlobal()
    {
        if (_gameClient.Connected)
            return;
        
        await StartConnect("185.212.119.242", 5000);
        _sceneSystem.SetScene(SceneType.Game);
    }
}