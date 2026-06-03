using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace TextureRenderSample;

internal sealed class TextureComponent : BaseComponent
{
    public string TexturePath { get; set; } = string.Empty;
    public Vector2 Size { get; set; } = new Vector2(128, 128);
    public Color Color { get; set; } = Color.White;
    public float ZIndex { get; set; } = 0f;
    public float SpinSpeed { get; set; } = 0f;

    private DateTime _lastUpdate;

    public override void Start()
    {
        _lastUpdate = DateTime.UtcNow;
    }

    public override void Update()
    {
        if (SpinSpeed != 0f)
        {
            var now = DateTime.UtcNow;
            Transform.Rotation += SpinSpeed * (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;
        }
    }

    public override void Render(DrawCommandBuffer buffer)
    {
        if (string.IsNullOrEmpty(TexturePath))
            return;

        buffer.DrawTexture(new DrawTextureCommand
        {
            Texture = RenderAssetManager.LoadTexture(TexturePath),
            Position = Transform.Position,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
            Color = Color,
            Size = Size,
        });
    }
}
