using System.Numerics;
using Silk.NET.Input;

namespace Aphelion.Core;

public enum InputState
{
    Down,
    Pressed,
    Up,
}

public enum MouseButton
{
    Left,
    Right,
    Middle,
}

public enum KeyCode
{
    Number0,
    Number1,
    Number2,
    Number3,
    Number4,
    Number5,
    Number6,
    Number7,
    Number8,
    Number9,
}

public class Input
{
    internal static ThreadLocal<Input> Instance = new(trackAllValues: true);
    
    private readonly IInputContext _inputContext;
    private readonly Dictionary<MouseButton, InputState> _mouseButtonStates = new();
    private readonly Dictionary<KeyCode, InputState> _keyCodeStates = new();
    
    private Vector2 _mousePosition;
    private Vector2 _mouseDelta;
    private Vector2 _mouseScrollDelta;

    internal static void CreateInstance(IInputContext inputContext)
    {
        var input = new Input(inputContext);
        Instance.Value = input;
        Instance.Values.Add(input);
    }
    
    internal static void Refresh()
    {
        Instance.Value!.RefreshInternally();
    }
    
    public static bool GetMouseButton(MouseButton mouseButton, InputState state)
    {
        var currentState = Instance.Value!._mouseButtonStates.GetValueOrDefault(mouseButton, InputState.Up);
        return state switch
        {
            InputState.Pressed => currentState is InputState.Down or InputState.Pressed,
            _ => currentState == state
        };
    }

    public static bool GetKey(KeyCode keyCode, InputState state)
    {
        var currentState = Instance.Value!._keyCodeStates.GetValueOrDefault(keyCode, InputState.Up);
        return state switch
        {
            InputState.Pressed => currentState is InputState.Down or InputState.Pressed,
            _ => currentState == state
        };
    }
    
    public static Vector2 GetMousePosition()
    {
        return Instance.Value!._mousePosition;
    }

    public static Vector2 GetMouseDelta()
    {
        return Instance.Value!._mouseDelta;
    }

    public static Vector2 GetMouseScrollDelta()
    {
        return Instance.Value!._mouseScrollDelta;
    }
    
    private Input(IInputContext inputContext)
    {
        _inputContext = inputContext;
        
        foreach (MouseButton mouseButton in Enum.GetValues(typeof(MouseButton)))
            _mouseButtonStates.Add(mouseButton, InputState.Up);

        foreach (var mouse in inputContext.Mice)
        {
            mouse.MouseDown += (_, mouseButton) =>
            {
                switch (mouseButton)
                {
                    case Silk.NET.Input.MouseButton.Left:
                        _mouseButtonStates[MouseButton.Left] = InputState.Down;
                        break;
                    case Silk.NET.Input.MouseButton.Right:
                        _mouseButtonStates[MouseButton.Right] = InputState.Down;
                        break;
                    case Silk.NET.Input.MouseButton.Middle:
                        _mouseButtonStates[MouseButton.Middle] = InputState.Down;
                        break;
                }
            };
            
            mouse.MouseUp += (sender, args) =>
            {
                switch (args)
                {
                    case Silk.NET.Input.MouseButton.Left:
                        _mouseButtonStates[MouseButton.Left] = InputState.Up;
                        break;
                    case Silk.NET.Input.MouseButton.Right:
                        _mouseButtonStates[MouseButton.Right] = InputState.Up;
                        break;
                    case Silk.NET.Input.MouseButton.Middle:
                        _mouseButtonStates[MouseButton.Middle] = InputState.Up;
                        break;
                }
            };
            
            mouse.MouseMove += (m, mouseDelta) =>
            {
                _mousePosition = m.Position;
                _mouseDelta += mouseDelta;
            };

            mouse.Scroll += (_, mouseScrollDelta) =>
            {
                _mouseScrollDelta += new Vector2(mouseScrollDelta.X, mouseScrollDelta.Y);
            };
        }
        
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            _keyCodeStates.Add(keyCode, InputState.Up);
        
        foreach (var keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += (_, key, _) =>
            {
                KeyCode? keyCode = ConvertKey(key);
                if (keyCode.HasValue)
                    _keyCodeStates[keyCode.Value] = InputState.Down;
            };

            keyboard.KeyUp += (_, key, _) =>
            {
                KeyCode? keyCode = ConvertKey(key);
                if (keyCode.HasValue)
                    _keyCodeStates[keyCode.Value] = InputState.Up;
            };
        }
    }
    
    private void RefreshInternally()
    {
        foreach (var mouseButtonState in _mouseButtonStates)
        {
            if (mouseButtonState.Value == InputState.Down)
                _mouseButtonStates[mouseButtonState.Key] = InputState.Pressed;
        }
        
        foreach (var keyCodeState in _keyCodeStates)
        {
            if (keyCodeState.Value == InputState.Down)
                _keyCodeStates[keyCodeState.Key] = InputState.Pressed;
        }
        
        _mouseDelta = Vector2.Zero;
        _mouseScrollDelta = Vector2.Zero;
    }

    private static KeyCode? ConvertKey(Key key)
    {
        return key switch
        {
            Key.Number0 => KeyCode.Number0,
            Key.Number1 => KeyCode.Number1,
            Key.Number2 => KeyCode.Number2,
            Key.Number3 => KeyCode.Number3,
            Key.Number4 => KeyCode.Number4,
            Key.Number5 => KeyCode.Number5,
            Key.Number6 => KeyCode.Number6,
            Key.Number7 => KeyCode.Number7,
            Key.Number8 => KeyCode.Number8,
            Key.Number9 => KeyCode.Number9,
            _ => null
        };
    }
}