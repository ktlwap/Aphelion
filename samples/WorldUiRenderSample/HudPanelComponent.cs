using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace WorldUiRenderSample;

internal sealed class HudPanelComponent : BaseComponent
{
    public Color Color { get; set; } = Color.White;
    public Vector2 Size { get; set; } = new(100, 100);
    public float ZIndex { get; set; } = 0f;

    public override void RenderUI(DrawCommandBuffer buffer)
    {
        buffer.DrawShape(new DrawShapeCommand
        {
            Position = Transform.Position,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
            Color = Color,
            Size = Size,
        });
    }
}