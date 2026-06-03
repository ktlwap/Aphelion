using System.Drawing;
using Aphelion.Core;
using Aphelion.Rendering;

namespace TextRenderSample;

internal sealed class TextComponent : BaseComponent
{
    public string Text { get; set; } = string.Empty;
    public string FontPath { get; set; } = string.Empty;
    public float FontSize { get; set; } = 32f;
    public Color Color { get; set; } = Color.White;
    public float ZIndex { get; set; } = 0f;

    public override void Render(DrawCommandBuffer buffer)
    {
        if (string.IsNullOrEmpty(Text) || string.IsNullOrEmpty(FontPath))
            return;

        buffer.DrawText(new DrawTextCommand
        {
            Text = Text,
            Font = RenderAssetManager.LoadFont(FontPath),
            FontSize = FontSize,
            Color = Color,
            Position = Transform.Position,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
        });
    }
}
