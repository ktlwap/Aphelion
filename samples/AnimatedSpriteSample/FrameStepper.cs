using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;
using Aphelion.Rendering;

namespace AnimatedSpriteSample;

internal sealed class FrameStepper : BaseComponent
{
    private const string FontPath = "Assets/Fonts/Roboto/Roboto-Regular.ttf";

    private AnimatedSpriteRenderer? _renderer;

    public override void Start()
    {
        _renderer = GameObject.GetComponent<AnimatedSpriteRenderer>();
    }

    public override void Update()
    {
        if (_renderer == null)
            return;

        if (Input.GetKey(KeyCode.Right, InputState.Down) || Input.GetKey(KeyCode.Space, InputState.Down))
            _renderer.CurrentFrame = (_renderer.CurrentFrame + 1) % _renderer.MaxFrames;

        if (Input.GetKey(KeyCode.Left, InputState.Down))
            _renderer.CurrentFrame = (_renderer.CurrentFrame - 1 + _renderer.MaxFrames) % _renderer.MaxFrames;

        if (Input.GetKey(KeyCode.R, InputState.Down))
            _renderer.CurrentFrame = 0;
    }

    public override void RenderUI(DrawCommandBuffer buffer)
    {
        var font = RenderAssetManager.LoadFont(FontPath);
        int frame = _renderer?.CurrentFrame ?? 0;
        int max = _renderer?.MaxFrames ?? 0;

        const float blockX = 20f;
        const float bottomBaseline = 615f;
        const float lineHeight = 26f;

        DrawLine(buffer, font, new Vector2(blockX, bottomBaseline - lineHeight * 4), "Manual frame stepper:", 18f, Color.White);
        DrawLine(buffer, font, new Vector2(blockX, bottomBaseline - lineHeight * 3), "  Right / Space  -  next frame",   16f, Color.LightGray);
        DrawLine(buffer, font, new Vector2(blockX, bottomBaseline - lineHeight * 2), "  Left           -  previous frame", 16f, Color.LightGray);
        DrawLine(buffer, font, new Vector2(blockX, bottomBaseline - lineHeight * 1), "  R              -  reset to frame 0", 16f, Color.LightGray);
        DrawLine(buffer, font, new Vector2(blockX, bottomBaseline),                  $"Current frame: {frame} / {Math.Max(0, max - 1)}", 18f, Color.Gold);
    }

    private static void DrawLine(DrawCommandBuffer buffer, Font font, Vector2 position, string text, float size, Color color)
    {
        buffer.DrawText(new DrawTextCommand
        {
            Text = text,
            Font = font,
            FontSize = size,
            Color = color,
            Position = position,
            Rotation = 0f,
            ZIndex = 0.9f,
        });
    }
}
