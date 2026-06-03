using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;
using TextureRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Texture Render Sample",
    Width = 1000,
    Height = 700,
    VSync = true,
});

const string DotNet = "Assets/Images/dotnet_logo.png";
const string WebGpu = "Assets/Images/webgpu_logo.png";

// Two large, untouched logos side by side at the top
Add("DotNetTop", DotNet, new Vector2(280, 160), new Vector2(220, 220), Color.White, 0f);
Add("WebGpuTop", WebGpu, new Vector2(720, 160), new Vector2(220, 220), Color.White, 0f);

// Tilted, tinted thumbnails along the middle
Add("Mid1", DotNet, new Vector2(120, 380), new Vector2(120, 120), Color.OrangeRed, MathF.PI * -0.15f);
Add("Mid2", WebGpu, new Vector2(280, 380), new Vector2(120, 120), Color.LimeGreen, MathF.PI * 0.10f);
Add("Mid3", DotNet, new Vector2(440, 380), new Vector2(120, 120), Color.DodgerBlue, MathF.PI * -0.25f);
Add("Mid4", WebGpu, new Vector2(600, 380), new Vector2(120, 120), Color.Gold, MathF.PI * 0.20f);
Add("Mid5", DotNet, new Vector2(760, 380), new Vector2(120, 120), Color.MediumPurple, MathF.PI * -0.05f);
Add("Mid6", WebGpu, new Vector2(920, 380), new Vector2(120, 120), Color.HotPink, MathF.PI * 0.30f);

// Two spinners at the bottom: opposite directions, half-transparent
AddSpinning("SpinnerLeft", DotNet, new Vector2(360, 580), new Vector2(160, 160), Color.FromArgb(180, 255, 255, 255),
    MathF.PI);
AddSpinning("SpinnerRight", WebGpu, new Vector2(640, 580), new Vector2(160, 160), Color.FromArgb(180, 200, 220, 255),
    -MathF.PI);

window.Run();

static void Add(string name, string path, Vector2 position, Vector2 size, Color color, float rotation)
{
    var go = GameObject.Instantiate(name);
    var c = go.AddComponent<TextureComponent>();
    c.TexturePath = path;
    c.Size = size;
    c.Color = color;
    go.Transform.Position = position;
    go.Transform.Rotation = rotation;
}

static void AddSpinning(string name, string path, Vector2 position, Vector2 size, Color color, float spinSpeed)
{
    var go = GameObject.Instantiate(name);
    var c = go.AddComponent<TextureComponent>();
    c.TexturePath = path;
    c.Size = size;
    c.Color = color;
    c.SpinSpeed = spinSpeed;
    go.Transform.Position = position;
}