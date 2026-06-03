using System.Numerics;
using Aphelion.Core;
using Moq;
using Silk.NET.Input;
using Xunit;
using MouseButton = Aphelion.Core.MouseButton;

namespace Aphelion.Tests.Core;

public class InputTests
{
    private readonly Mock<IInputContext> _inputContextMock;
    private readonly Mock<IMouse> _mouseMock;
    private readonly Mock<IKeyboard> _keyboardMock;

    public InputTests()
    {
        _inputContextMock = new Mock<IInputContext>();
        _mouseMock = new Mock<IMouse>();
        _keyboardMock = new Mock<IKeyboard>();

        _inputContextMock.Setup(x => x.Mice).Returns(new List<IMouse> { _mouseMock.Object }.AsReadOnly());
        _inputContextMock.Setup(x => x.Keyboards).Returns(new List<IKeyboard> { _keyboardMock.Object }.AsReadOnly());
    }

    [Fact]
    public void InitializeInstance_CreatesNewInputInstance()
    {
        Input.CreateInstance(_inputContextMock.Object);
    }

    [Fact]
    public void GetMouseButton_ReturnsCorrectState()
    {
        Input.CreateInstance(_inputContextMock.Object);

        // Simulate MouseDown
        _mouseMock.Raise(m => m.MouseDown += null, _mouseMock.Object, Silk.NET.Input.MouseButton.Left);

        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Down));
        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Pressed));
        Assert.False(Input.GetMouseButton(MouseButton.Left, InputState.Up));
    }

    [Fact]
    public void Refresh_MovesDownToPressed()
    {
        Input.CreateInstance(_inputContextMock.Object);

        _mouseMock.Raise(m => m.MouseDown += null, _mouseMock.Object, Silk.NET.Input.MouseButton.Left);
        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Down));

        Input.Refresh();

        Assert.False(Input.GetMouseButton(MouseButton.Left, InputState.Down));
        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Pressed));
    }

    [Fact]
    public void GetMousePosition_ReturnsCorrectValue()
    {
        Input.CreateInstance(_inputContextMock.Object);
        var expectedPosition = new Vector2(100, 200);
        _mouseMock.Setup(m => m.Position).Returns(expectedPosition);

        _mouseMock.Raise(m => m.MouseMove += null, _mouseMock.Object, Vector2.Zero);

        Assert.Equal(expectedPosition, Input.GetMousePosition());
    }

    [Fact]
    public void GetMouseDelta_ReturnsCorrectValue()
    {
        Input.CreateInstance(_inputContextMock.Object);
        var delta = new Vector2(5, 10);

        _mouseMock.Raise(m => m.MouseMove += null, _mouseMock.Object, delta);

        Assert.Equal(delta, Input.GetMouseDelta());
    }

    [Fact]
    public void Refresh_ResetsDelta()
    {
        Input.CreateInstance(_inputContextMock.Object);
        _mouseMock.Raise(m => m.MouseMove += null, _mouseMock.Object, new Vector2(5, 10));

        Input.Refresh();

        Assert.Equal(Vector2.Zero, Input.GetMouseDelta());
    }

    [Fact]
    public void MouseDelta_AccumulatesBetweenRefreshes()
    {
        Input.CreateInstance(_inputContextMock.Object);
        _mouseMock.Raise(m => m.MouseMove += null, _mouseMock.Object, new Vector2(5, 10));
        _mouseMock.Raise(m => m.MouseMove += null, _mouseMock.Object, new Vector2(2, 3));

        Assert.Equal(new Vector2(7, 13), Input.GetMouseDelta());
    }

    [Fact]
    public void GetMouseButton_PressedState_ReturnsTrueIfDownOrPressed()
    {
        Input.CreateInstance(_inputContextMock.Object);

        // State starts Up
        Assert.False(Input.GetMouseButton(MouseButton.Left, InputState.Pressed));

        // Set to Down
        _mouseMock.Raise(m => m.MouseDown += null, _mouseMock.Object, Silk.NET.Input.MouseButton.Left);
        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Pressed));
        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Down));

        // Refresh moves Down to Pressed
        Input.Refresh();
        Assert.True(Input.GetMouseButton(MouseButton.Left, InputState.Pressed));
        Assert.False(Input.GetMouseButton(MouseButton.Left, InputState.Down));
    }

    [Fact]
    public void KeyDown_SetsStateToDownAndPressed()
    {
        Input.CreateInstance(_inputContextMock.Object);

        _keyboardMock.Raise(k => k.KeyDown += null, _keyboardMock.Object, Key.Number5, 0);

        Assert.True(Input.GetKey(KeyCode.Number5, InputState.Down));
        Assert.True(Input.GetKey(KeyCode.Number5, InputState.Pressed));

        Input.Refresh();
        Assert.False(Input.GetKey(KeyCode.Number5, InputState.Down));
        Assert.True(Input.GetKey(KeyCode.Number5, InputState.Pressed));
    }
}