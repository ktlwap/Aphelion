using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace AnimatedSpriteSample;

internal sealed class AnimatedSpriteScene : BaseScene
{
    public const int ScreenWidth = 960;
    public const int ScreenHeight = 640;

    // Sprite sheet: 5 columns x 5 rows, 23 explosion frames (last two cells empty).
    private const string ExplosionSheet = "Assets/Images/explosion.png";
    private const int SheetRows = 5;
    private const int SheetColumns = 5;
    private const int FrameCount = 23;

    public override void Start()
    {
        // Three auto-playing explosions at different speeds.
        AddAutoPlaying("FastBoom",   new Vector2(180, 200), new Vector2(192, 192), 24f);
        AddAutoPlaying("MediumBoom", new Vector2(480, 200), new Vector2(192, 192), 12f);
        AddAutoPlaying("SlowBoom",   new Vector2(780, 200), new Vector2(192, 192),  6f);

        // Manually-controlled explosion: arrow keys / space step through frames.
        AddManuallyControlled("Manual", new Vector2(480, 460), new Vector2(256, 256));

        GameObject.Instantiate("Camera").AddComponent<Camera>();
    }

    private static void AddAutoPlaying(string name, Vector2 position, Vector2 size, float fps)
    {
        var go = GameObject.Instantiate(name);
        go.Transform.Position = position;

        var renderer = go.AddComponent<AnimatedSpriteRenderer>();
        renderer.Rows = SheetRows;
        renderer.Columns = SheetColumns;
        renderer.MaxFrames = FrameCount;
        renderer.Size = size;
        renderer.Anchor = new Vector2(0.5f, 0.5f);
        renderer.AutoPlay = true;
        renderer.FramesPerSecond = fps;
        renderer.LoadImage(ExplosionSheet);
    }

    private static void AddManuallyControlled(string name, Vector2 position, Vector2 size)
    {
        var go = GameObject.Instantiate(name);
        go.Transform.Position = position;

        var renderer = go.AddComponent<AnimatedSpriteRenderer>();
        renderer.Rows = SheetRows;
        renderer.Columns = SheetColumns;
        renderer.MaxFrames = FrameCount;
        renderer.Size = size;
        renderer.Anchor = new Vector2(0.5f, 0.5f);
        renderer.AutoPlay = false;
        renderer.LoadImage(ExplosionSheet);

        go.AddComponent<FrameStepper>();
    }
}
