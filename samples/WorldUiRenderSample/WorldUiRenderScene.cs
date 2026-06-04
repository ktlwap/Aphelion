using System.Drawing;
using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace WorldUiRenderSample;

internal sealed class WorldUiRenderScene : BaseScene
{
    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 700;

    private const string Roboto = "Assets/Fonts/Roboto-VariableFont_wdth,wght.ttf";

    public override void Start()
    {
        // -------------------------------------------------------------------
        // World layer — components push into the world buffer from Render(buffer).
        // The shader transforms these by uniforms.projection_view (see shader.wgsl).
        // -------------------------------------------------------------------

        AddWorldQuad("WorldBg", new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
            new Vector2(ScreenWidth, ScreenHeight), Color.FromArgb(255, 18, 24, 36), zIndex: -10f);

        for (int x = 0; x < 10; x++)
        for (int y = 0; y < 7; y++)
        {
            AddWorldQuad($"Tile_{x}_{y}",
                new Vector2(50 + x * 100, 50 + y * 100),
                new Vector2(88, 88),
                Color.FromArgb(255, 36, 48, 72),
                zIndex: -5f);
        }

        AddSpinningWorldQuad("Player", new Vector2(500, 350), new Vector2(90, 90),
            Color.Gold, spinSpeed: MathF.PI * 0.5f, zIndex: 2f);

        AddSpinningWorldQuad("EnemyTL", new Vector2(200, 200), new Vector2(60, 60),
            Color.OrangeRed, -MathF.PI * 0.7f, zIndex: 1f);
        AddSpinningWorldQuad("EnemyTR", new Vector2(800, 200), new Vector2(60, 60),
            Color.LimeGreen, MathF.PI * 0.6f, zIndex: 1f);
        AddSpinningWorldQuad("EnemyBL", new Vector2(200, 500), new Vector2(60, 60),
            Color.DodgerBlue, MathF.PI * 0.4f, zIndex: 1f);
        AddSpinningWorldQuad("EnemyBR", new Vector2(800, 500), new Vector2(60, 60),
            Color.HotPink, -MathF.PI * 0.5f, zIndex: 1f);

        // -------------------------------------------------------------------
        // UI layer — components push into the UI buffer from RenderUI(buffer).
        // Drawn after world, always anchored to screen space.
        // -------------------------------------------------------------------

        AddHudPanel("TopBar", new Vector2(ScreenWidth / 2f, 30), new Vector2(ScreenWidth, 60),
            Color.FromArgb(210, 0, 0, 0));
        AddHudText("Title", new Vector2(20, 18), "Aphelion — World + UI Layers",
            24f, Color.White);
        var fpsGo = GameObject.Instantiate("FpsCounter");
        var fps = fpsGo.AddComponent<FpsCounterComponent>();
        fps.FontPath = Roboto;
        fps.FontSize = 24f;
        fps.Color = Color.Gold;
        fpsGo.Transform.Position = new Vector2(ScreenWidth - 180, 18);

        AddHudPanel("BottomBar", new Vector2(ScreenWidth / 2f, ScreenHeight - 30),
            new Vector2(ScreenWidth, 60), Color.FromArgb(210, 0, 0, 0));
        AddHudText("Hint", new Vector2(20, ScreenHeight - 44),
            "Render(buffer) → world layer    RenderUI(buffer) → UI layer", 20f,
            Color.LightGray);

        AddHudPanel("Crosshair", new Vector2(ScreenWidth / 2f, ScreenHeight / 2f),
            new Vector2(10, 10), Color.FromArgb(220, 255, 255, 255), zIndex: 5f);

        // -------------------------------------------------------------------
        // Camera — pans and zooms continuously. The world matrix is built as
        // Camera.Main.View * projection, so world quads + text move with the camera
        // while every HUD element above stays anchored to the screen.
        // -------------------------------------------------------------------

        var cameraGameObject = GameObject.Instantiate("CameraController");
        cameraGameObject.AddComponent<Camera>();
        cameraGameObject.AddComponent<CameraControllerComponent>();
    }

    private static void AddWorldQuad(string name, Vector2 position, Vector2 size, Color color, float zIndex = 0f)
    {
        var go = GameObject.Instantiate(name);
        var c = go.AddComponent<WorldQuadComponent>();
        c.Color = color;
        c.Size = size;
        c.ZIndex = zIndex;
        go.Transform.Position = position;
    }

    private static void AddSpinningWorldQuad(string name, Vector2 position, Vector2 size, Color color, float spinSpeed,
        float zIndex = 0f)
    {
        var go = GameObject.Instantiate(name);
        var c = go.AddComponent<WorldQuadComponent>();
        c.Color = color;
        c.Size = size;
        c.ZIndex = zIndex;
        c.SpinSpeed = spinSpeed;
        go.Transform.Position = position;
    }

    private static void AddHudPanel(string name, Vector2 position, Vector2 size, Color color, float zIndex = 0f)
    {
        var go = GameObject.Instantiate(name);
        var c = go.AddComponent<HudPanelComponent>();
        c.Color = color;
        c.Size = size;
        c.ZIndex = zIndex;
        go.Transform.Position = position;
    }

    private static void AddHudText(string name, Vector2 position, string text, float fontSize, Color color,
        float zIndex = 1f)
    {
        var go = GameObject.Instantiate(name);
        var c = go.AddComponent<HudTextComponent>();
        c.Text = text;
        c.FontPath = Roboto;
        c.FontSize = fontSize;
        c.Color = color;
        c.ZIndex = zIndex;
        go.Transform.Position = position;
    }
}
