using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace ColliderClickSample;

internal sealed class ClickHandlerComponent : BaseComponent
{
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }

    public static Vector2 MouseWorldPosition { get; private set; }

    public override void Update()
    {
        var camera = Camera.Main;
        if (camera is null)
            return;

        MouseWorldPosition = camera.ScreenToWorldPosition(
            Input.GetMousePosition(), ScreenWidth, ScreenHeight);

        if (!Input.GetMouseButton(MouseButton.Left, InputState.Down))
            return;

        var hit = Physics2D.RayCast(MouseWorldPosition);
        hit?.Collider.GameObject
            .GetComponentNullSafe<ClickableRectangleComponent>()
            ?.OnClick();
    }
}