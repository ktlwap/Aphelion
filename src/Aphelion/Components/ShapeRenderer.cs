using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace Aphelion.Components;

public sealed class ShapeRenderer : BaseComponent
{
    public bool IsVisible { get; set; }
    public Vector2 Size { get; set; }
    public Vector2 Anchor { get; set; }
    public Color Color { get; set; }
    public float ZIndex { get; set; }
    
    public ShapeRenderer()
    {
        IsVisible = true;
        Size = Vector2.One;
        Anchor = Vector2.Zero;
        Color = Color.White;
        ZIndex = 0;
    }
    
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