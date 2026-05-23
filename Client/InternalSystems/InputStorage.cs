using Client.Data;
using Client.Systems.PredictSystems;
using Hypercube.Core.Ecs;
using Hypercube.Core.Input;
using Hypercube.Core.Input.Handler;
using Hypercube.Utilities.Dependencies;

namespace Client.InternalSystems;

public class InputStorage : EntitySystem
{
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    private HashSet<Input> _currentInputs = [];
    private readonly HashSet<Input>[] _buffer = new HashSet<Input>[PredictHelper.Capacity];

    public override void Initialize()
    {
        for (var i = 0; i < _buffer.Length; i++)
            _buffer[i] = [];
    }
    
    public void CaptureActualInputs(long tick)
    {
        _currentInputs = _buffer[tick % PredictHelper.Capacity];
        _currentInputs.Clear();
        
        DetectInputs(_currentInputs);
    }

    public void SetMockInput(long tick)
    {
        _currentInputs = _buffer[tick % PredictHelper.Capacity];
    }

    public bool HasInput(Input input)
    {
        return _currentInputs.Contains(input);
    }

    private void DetectInputs(HashSet<Input> inputs)
    {
        if (_inputHandler.IsKeyHeld(Key.W))
            inputs.Add(Input.MoveUp);
        
        if (_inputHandler.IsKeyHeld(Key.S))
            inputs.Add(Input.MoveDown);
        
        if (_inputHandler.IsKeyHeld(Key.D))
            inputs.Add(Input.MoveRight);
        
        if (_inputHandler.IsKeyHeld(Key.A))
            inputs.Add(Input.MoveLeft);
        
        if (_inputHandler.IsMouseButtonHeld(MouseButton.Left))
            inputs.Add(Input.Attack);
    }
}