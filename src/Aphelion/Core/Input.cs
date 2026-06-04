using System.Numerics;
using Silk.NET.GLFW;

namespace Aphelion.Core;

public enum InputState
{
    Down,
    Pressed,
    Released,
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
        Space,
        Apostrophe,
        Comma,
        Minus,
        Period,
        Slash,
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
        Semicolon,
        Equal,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        LeftBracket,
        BackSlash,
        RightBracket,
        GraveAccent,
        World1,
        World2,
        Escape,
        Enter,
        Tab,
        Backspace,
        Insert,
        Delete,
        Right,
        Left,
        Down,
        Up,
        PageUp,
        PageDown,
        Home,
        End,
        CapsLock,
        ScrollLock,
        NumLock,
        PrintScreen,
        Pause,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        F20,
        F21,
        F22,
        F23,
        F24,
        F25,
        Keypad0,
        Keypad1,
        Keypad2,
        Keypad3,
        Keypad4,
        Keypad5,
        Keypad6,
        Keypad7,
        Keypad8,
        Keypad9,
        KeypadDecimal,
        KeypadDivide,
        KeypadMultiply,
        KeypadSubtract,
        KeypadAdd,
        KeypadEnter,
        KeypadEqual,
        ShiftLeft,
        ControlLeft,
        AltLeft,
        SuperLeft,
        ShiftRight,
        ControlRight,
        AltRight,
        SuperRight,
        Menu,
}

public unsafe class Input
{
    private static ThreadLocal<Input> Instance = new(trackAllValues: true);

    internal static Input Current => Instance.Value!;

    private readonly Glfw _glfw;
    private readonly WindowHandle* _pWindowHandle;
    private readonly Dictionary<MouseButton, InputState> _mouseButtonStates;
    private readonly Dictionary<KeyCode, InputState> _keyCodeStates;
    private Vector2 _mousePosition;
    private Vector2 _mouseWheelDelta;

    internal Input(Glfw glfw, WindowHandle* pWindowHandle)
    {
        Instance.Value = this;
        
        _glfw = glfw;
        _pWindowHandle = pWindowHandle;
        
        _mouseButtonStates = new Dictionary<MouseButton, InputState>();
        foreach (var mouseButton in Enum.GetValues<MouseButton>())
        {
            _mouseButtonStates.Add(mouseButton, InputState.Up);
        }
        
        _keyCodeStates = new Dictionary<KeyCode, InputState>();
        foreach (var keyCode in Enum.GetValues<KeyCode>())
        {
            _keyCodeStates.Add(keyCode, InputState.Up);
        }

        _glfw.SetScrollCallback(_pWindowHandle, (_, x, y) =>
            _mouseWheelDelta = new Vector2((float)x, (float)y));
        
        _glfw.SetCursorPosCallback(_pWindowHandle, (_, x, y) => 
            _mousePosition = new Vector2((float)x, (float)y));

        Refresh();
    }

    internal void Refresh()
    {
        _glfw.PollEvents();
        
        foreach (var mouseButtonState in _mouseButtonStates)
        {
            var inputAction = (InputAction)_glfw.GetMouseButton(_pWindowHandle, GetGlfwMouseButtonCode(mouseButtonState.Key));
            _mouseButtonStates[mouseButtonState.Key] = ConvertToInputState(inputAction, mouseButtonState.Value);
        }
        
        foreach (var keyCodeState in _keyCodeStates)
        {
            var glfwKeys = ConvertKeyCodeToGlfwKeys(keyCodeState.Key);
            var inputAction = (InputAction)_glfw.GetKey(_pWindowHandle, glfwKeys);
            _keyCodeStates[keyCodeState.Key] = ConvertToInputState(inputAction, keyCodeState.Value);
        }
    }

    public Vector2 GetMousePosition()
    {
        return _mousePosition;
    }
    
    public Vector2 GetMouseWheelDelta()
    {
        return _mouseWheelDelta;
    }

    public bool GetMouseButton(MouseButton mouseButton, InputState inputState)
    {
        var currentState = _mouseButtonStates[mouseButton];
        if (InputState.Pressed == inputState)
            return currentState is InputState.Down or InputState.Pressed;

        return currentState == inputState;
    }

    public bool GetKey(KeyCode keyCode, InputState inputState)
    {
        var currentState = _keyCodeStates[keyCode];
        if (InputState.Pressed == inputState)
            return currentState is InputState.Down or InputState.Pressed;
        
        return currentState == inputState;
    }

    private static int GetGlfwMouseButtonCode(MouseButton mouseButton)
    {
        return mouseButton switch
        {
            MouseButton.Left => 0,
            MouseButton.Right => 1,
            MouseButton.Middle => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(mouseButton), mouseButton, null)
        };
    }

    private static InputState ConvertToInputState(InputAction inputAction, InputState currentState)
    {
        switch (inputAction)
        {
            case InputAction.Press:
                return currentState == InputState.Up ? InputState.Down : InputState.Pressed;
            case InputAction.Release:
                return currentState == InputState.Down ? InputState.Released : InputState.Up;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static Keys ConvertKeyCodeToGlfwKeys(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.Space => Keys.Space,
            KeyCode.Apostrophe => Keys.Apostrophe,
            KeyCode.Comma => Keys.Comma,
            KeyCode.Minus => Keys.Minus,
            KeyCode.Period => Keys.Period,
            KeyCode.Slash => Keys.Slash,
            KeyCode.Number0 => Keys.Number0,
            KeyCode.Number1 => Keys.Number1,
            KeyCode.Number2 => Keys.Number2,
            KeyCode.Number3 => Keys.Number3,
            KeyCode.Number4 => Keys.Number4,
            KeyCode.Number5 => Keys.Number5,
            KeyCode.Number6 => Keys.Number6,
            KeyCode.Number7 => Keys.Number7,
            KeyCode.Number8 => Keys.Number8,
            KeyCode.Number9 => Keys.Number9,
            KeyCode.Semicolon => Keys.Semicolon,
            KeyCode.Equal => Keys.Equal,
            KeyCode.A => Keys.A,
            KeyCode.B => Keys.B,
            KeyCode.C => Keys.C,
            KeyCode.D => Keys.D,
            KeyCode.E => Keys.E,
            KeyCode.F => Keys.F,
            KeyCode.G => Keys.G,
            KeyCode.H => Keys.H,
            KeyCode.I => Keys.I,
            KeyCode.J => Keys.J,
            KeyCode.K => Keys.K,
            KeyCode.L => Keys.L,
            KeyCode.M => Keys.M,
            KeyCode.N => Keys.N,
            KeyCode.O => Keys.O,
            KeyCode.P => Keys.P,
            KeyCode.Q => Keys.Q,
            KeyCode.R => Keys.R,
            KeyCode.S => Keys.S,
            KeyCode.T => Keys.T,
            KeyCode.U => Keys.U,
            KeyCode.V => Keys.V,
            KeyCode.W => Keys.W,
            KeyCode.X => Keys.X,
            KeyCode.Y => Keys.Y,
            KeyCode.Z => Keys.Z,
            KeyCode.LeftBracket => Keys.LeftBracket,
            KeyCode.BackSlash => Keys.BackSlash,
            KeyCode.RightBracket => Keys.RightBracket,
            KeyCode.GraveAccent => Keys.GraveAccent,
            KeyCode.World1 => Keys.World1,
            KeyCode.World2 => Keys.World2,
            KeyCode.Escape => Keys.Escape,
            KeyCode.Enter => Keys.Enter,
            KeyCode.Tab => Keys.Tab,
            KeyCode.Backspace => Keys.Backspace,
            KeyCode.Insert => Keys.Insert,
            KeyCode.Delete => Keys.Delete,
            KeyCode.Right => Keys.Right,
            KeyCode.Left => Keys.Left,
            KeyCode.Down => Keys.Down,
            KeyCode.Up => Keys.Up,
            KeyCode.PageUp => Keys.PageUp,
            KeyCode.PageDown => Keys.PageDown,
            KeyCode.Home => Keys.Home,
            KeyCode.End => Keys.End,
            KeyCode.CapsLock => Keys.CapsLock,
            KeyCode.ScrollLock => Keys.ScrollLock,
            KeyCode.NumLock => Keys.NumLock,
            KeyCode.PrintScreen => Keys.PrintScreen,
            KeyCode.Pause => Keys.Pause,
            KeyCode.F1 => Keys.F1,
            KeyCode.F2 => Keys.F2,
            KeyCode.F3 => Keys.F3,
            KeyCode.F4 => Keys.F4,
            KeyCode.F5 => Keys.F5,
            KeyCode.F6 => Keys.F6,
            KeyCode.F7 => Keys.F7,
            KeyCode.F8 => Keys.F8,
            KeyCode.F9 => Keys.F9,
            KeyCode.F10 => Keys.F10,
            KeyCode.F11 => Keys.F11,
            KeyCode.F12 => Keys.F12,
            KeyCode.F13 => Keys.F13,
            KeyCode.F14 => Keys.F14,
            KeyCode.F15 => Keys.F15,
            KeyCode.F16 => Keys.F16,
            KeyCode.F17 => Keys.F17,
            KeyCode.F18 => Keys.F18,
            KeyCode.F19 => Keys.F19,
            KeyCode.F20 => Keys.F20,
            KeyCode.F21 => Keys.F21,
            KeyCode.F22 => Keys.F22,
            KeyCode.F23 => Keys.F23,
            KeyCode.F24 => Keys.F24,
            KeyCode.F25 => Keys.F25,
            KeyCode.Keypad0 => Keys.Keypad0,
            KeyCode.Keypad1 => Keys.Keypad1,
            KeyCode.Keypad2 => Keys.Keypad2,
            KeyCode.Keypad3 => Keys.Keypad3,
            KeyCode.Keypad4 => Keys.Keypad4,
            KeyCode.Keypad5 => Keys.Keypad5,
            KeyCode.Keypad6 => Keys.Keypad6,
            KeyCode.Keypad7 => Keys.Keypad7,
            KeyCode.Keypad8 => Keys.Keypad8,
            KeyCode.Keypad9 => Keys.Keypad9,
            KeyCode.KeypadDecimal => Keys.KeypadDecimal,
            KeyCode.KeypadDivide => Keys.KeypadDivide,
            KeyCode.KeypadMultiply => Keys.KeypadMultiply,
            KeyCode.KeypadSubtract => Keys.KeypadSubtract,
            KeyCode.KeypadAdd => Keys.KeypadAdd,
            KeyCode.KeypadEnter => Keys.KeypadEnter,
            KeyCode.KeypadEqual => Keys.KeypadEqual,
            KeyCode.ShiftLeft => Keys.ShiftLeft,
            KeyCode.ControlLeft => Keys.ControlLeft,
            KeyCode.AltLeft => Keys.AltLeft,
            KeyCode.SuperLeft => Keys.SuperLeft,
            KeyCode.ShiftRight => Keys.ShiftRight,
            KeyCode.ControlRight => Keys.ControlRight,
            KeyCode.AltRight => Keys.AltRight,
            KeyCode.SuperRight => Keys.SuperRight,
            KeyCode.Menu => Keys.Menu,
            _ => throw new ArgumentOutOfRangeException(nameof(keyCode), keyCode, null)
        };
    }
}