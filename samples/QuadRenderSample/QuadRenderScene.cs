using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace QuadRenderSample;

internal sealed class QuadRenderScene : BaseScene
{
    public const int ScreenWidth = 800;
    public const int ScreenHeight = 600;

    public override void Start()
    {
        // Dark gray background filling the whole window
        var bg = GameObject.Instantiate("Background");
        var bgComp = bg.AddComponent<QuadComponent>();
        bgComp.Color = Color.FromArgb(1f, 30f / 255f, 30f / 255f, 30f / 255f);
        bgComp.Size = new Vector2(ScreenWidth, ScreenHeight);
        bgComp.ZIndex = -1f;
        bg.Transform.Position = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);

        // Red quad — top-left
        var red = GameObject.Instantiate("RedQuad");
        var redComp = red.AddComponent<QuadComponent>();
        redComp.Color = Color.Red;
        redComp.Size = new Vector2(150, 150);
        red.Transform.Position = new Vector2(200, 150);

        // Green quad — top-right
        var green = GameObject.Instantiate("GreenQuad");
        var greenComp = green.AddComponent<QuadComponent>();
        greenComp.Color = Color.LimeGreen;
        greenComp.Size = new Vector2(150, 150);
        green.Transform.Position = new Vector2(600, 150);

        // Blue quad — bottom-left
        var blue = GameObject.Instantiate("BlueQuad");
        var blueComp = blue.AddComponent<QuadComponent>();
        blueComp.Color = Color.DodgerBlue;
        blueComp.Size = new Vector2(150, 150);
        blue.Transform.Position = new Vector2(200, 450);

        // Yellow quad — bottom-right
        var yellow = GameObject.Instantiate("YellowQuad");
        var yellowComp = yellow.AddComponent<QuadComponent>();
        yellowComp.Color = Color.Gold;
        yellowComp.Size = new Vector2(150, 150);
        yellow.Transform.Position = new Vector2(600, 450);

        // White quad spinning at center, drawn on top
        var spinner = GameObject.Instantiate("Spinner");
        var spinComp = spinner.AddComponent<QuadComponent>();
        spinComp.Color = Color.White;
        spinComp.Size = new Vector2(100, 100);
        spinComp.ZIndex = 1f;
        spinComp.SpinSpeed = MathF.PI; // half-turn per second
        spinner.Transform.Position = new Vector2(ScreenWidth / 2f, ScreenHeight / 2f);

        GameObject.Instantiate("Camera").AddComponent<Camera>();
    }
}
