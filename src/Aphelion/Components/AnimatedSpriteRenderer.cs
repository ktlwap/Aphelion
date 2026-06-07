using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace Aphelion.Components;

public sealed class AnimatedSpriteRenderer : BaseComponent
{
    private Texture? _texture;
    private float _elapsed;
    private int _currentFrame;

    public bool IsVisible { get; set; } = true;
    public Vector2 Offset { get; set; } = Vector2.Zero;
    public Vector2 Size { get; set; } = Vector2.One;
    public Vector2 Anchor { get; set; } = Vector2.Zero;
    public Color Color { get; set; } = Color.White;
    public float ZIndex { get; set; } = 0f;

    public int Rows { get; set; } = 1;
    public int Columns { get; set; } = 1;
    public int MaxFrames { get; set; } = 1;

    public bool AutoPlay { get; set; } = false;
    public float FramesPerSecond { get; set; } = 12f;

    public int CurrentFrame
    {
        get => _currentFrame;
        set => _currentFrame = MaxFrames <= 0 ? 0 : Math.Clamp(value, 0, MaxFrames - 1);
    }

    public void LoadImage(string fileName)
    {
        _texture = RenderAssetManager.LoadTexture(fileName);
    }

    public override void Render(DrawCommandBuffer buffer)
    {
        if (AutoPlay && FramesPerSecond > 0f && MaxFrames > 0)
        {
            _elapsed += Time.DeltaF;
            var frameDuration = 1f / FramesPerSecond;
            if (_elapsed >= frameDuration)
            {
                _elapsed -= frameDuration;
                _currentFrame = (_currentFrame + 1) % MaxFrames;
            }
        }

        if (!IsVisible || _texture == null)
            return;

        var frameW = 1f / Columns;
        var frameH = 1f / Rows;
        var col = _currentFrame % Columns;
        var row = _currentFrame / Columns;

        var uvMin = new Vector2(col * frameW, row * frameH);
        var uvMax = new Vector2((col + 1) * frameW, (row + 1) * frameH);

        var origin = Size * Transform.Scale * Anchor;

        buffer.DrawTexture(new DrawTextureCommand
        {
            Position = Transform.Position + Offset - origin,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
            Color = Color,
            Texture = _texture,
            Size = Size * Transform.Scale,
            UvMin = uvMin,
            UvMax = uvMax,
        });
    }
}
