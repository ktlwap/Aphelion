using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace TextureRenderSample;

internal sealed class TextureRenderScene : BaseScene
{
    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 700;

    private const string DotNet = "Assets/Images/dotnet_logo.png";
    private const string WebGpu = "Assets/Images/webgpu_logo.png";

    public override void Start()
    {
        // Two large, untouched logos side by side at the top
        Add("DotNetTop", DotNet, new Vector2(280, 160), new Vector2(220, 220), Color.White, 0f);
        Add("WebGpuTop", WebGpu, new Vector2(720, 160), new Vector2(220, 220), Color.White, 0f);

        // Tilted, tinted thumbnails along the middle
        Add("Mid1", DotNet, new Vector2(120, 380), new Vector2(120, 120), Color.OrangeRed, -27f);
        Add("Mid2", WebGpu, new Vector2(280, 380), new Vector2(120, 120), Color.LimeGreen, 18f);
        Add("Mid3", DotNet, new Vector2(440, 380), new Vector2(120, 120), Color.DodgerBlue, -45f);
        Add("Mid4", WebGpu, new Vector2(600, 380), new Vector2(120, 120), Color.Gold, 36f);
        Add("Mid5", DotNet, new Vector2(760, 380), new Vector2(120, 120), Color.MediumPurple, -9f);
        Add("Mid6", WebGpu, new Vector2(920, 380), new Vector2(120, 120), Color.HotPink, 54f);

        // Two spinners at the bottom: opposite directions, half-transparent
        AddSpinning("SpinnerLeft", DotNet, new Vector2(360, 580), new Vector2(160, 160),
            Color.FromArgb(180f / 255f, 1f, 1f, 1f), 180f);
        AddSpinning("SpinnerRight", WebGpu, new Vector2(640, 580), new Vector2(160, 160),
            Color.FromArgb(180f / 255f, 200f / 255f, 220f / 255f, 1f), -180f);

        GameObject.Instantiate("Camera").AddComponent<Camera>();
    }

    private static void Add(string name, string path, Vector2 position, Vector2 size, Color color, float rotation)
    {
        var go = GameObject.Instantiate(name);
        var c = go.AddComponent<TextureComponent>();
        c.TexturePath = path;
        c.Size = size;
        c.Color = color;
        go.Transform.Position = position;
        go.Transform.Rotation = rotation;
    }

    private static void AddSpinning(string name, string path, Vector2 position, Vector2 size, Color color,
        float spinSpeed)
    {
        var go = GameObject.Instantiate(name);
        var c = go.AddComponent<TextureComponent>();
        c.TexturePath = path;
        c.Size = size;
        c.Color = color;
        c.SpinSpeed = spinSpeed;
        go.Transform.Position = position;
    }
}
