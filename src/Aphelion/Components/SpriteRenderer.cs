using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace Aphelion.Components;

public sealed class SpriteRenderer : BaseComponent
{
    private Texture? _texture;

    public bool IsVisible { get; set; } = true;
    public Vector2 Offset { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.One;
    public Vector2 Anchor { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;
    public float ZIndex { get; set; } = 0f;

    public void LoadImage(string fileName)
    {
        _texture = RenderAssetManager.LoadTexture(fileName);
    }

    public override void Render(DrawCommandBuffer buffer)
    {
        if (!IsVisible || _texture == null)
            return;

        var origin = Size * Transform.Scale * Anchor;

        buffer.DrawTexture(new DrawTextureCommand()
        {
            Position = Transform.Position + Offset - origin,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
            Color = Color,
            Texture = _texture,
            Size = Size * Transform.Scale,
        });
    }
}
