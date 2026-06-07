using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace Aphelion.Components;

public sealed class ShapeRenderer : BaseComponent
{
    public bool IsVisible { get; set; } = true;
    public Vector2 Size { get; set; } = Vector2.One;
    public Vector2 Anchor { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;
    public float ZIndex { get; set; } = 0;

    public override void Render(DrawCommandBuffer buffer)
    {
        if (!IsVisible)
            return;
        
        buffer.DrawShape(new DrawShapeCommand()
        {
            Position = Transform.Position,
            Size = Size * Transform.Scale,
            Rotation = Transform.Rotation,
            Color = Color,
            ZIndex = ZIndex
        });
    }
}