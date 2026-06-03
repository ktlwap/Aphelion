using System.Drawing;
using Aphelion.Core;
using Aphelion.Rendering;

namespace WorldUiRenderSample;

internal sealed class FpsCounterComponent : BaseComponent
{
    public string FontPath { get; set; } = string.Empty;
    public float FontSize { get; set; } = 24f;
    public Color Color { get; set; } = Color.Gold;
    public float ZIndex { get; set; } = 1f;

    private DateTime _lastSample = DateTime.UtcNow;
    private int _framesSinceSample;
    private float _fps;

    public override void Update()
    {
        _framesSinceSample++;
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastSample).TotalSeconds;
        if (elapsed >= 0.25)
        {
            _fps = (float)(_framesSinceSample / elapsed);
            _framesSinceSample = 0;
            _lastSample = now;
        }
    }

    public override void RenderUI(DrawCommandBuffer buffer)
    {
        if (string.IsNullOrEmpty(FontPath))
            return;

        buffer.DrawText(new DrawTextCommand
        {
            Text = $"FPS: {_fps:N0}",
            Font = RenderAssetManager.LoadFont(FontPath),
            FontSize = FontSize,
            Color = Color,
            Position = Transform.Position,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
        });
    }
}
