using System.Drawing;
using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;
using Aphelion.Physics;
using Aphelion.Rendering;
using ColliderClickSample;

const int Width = 1000;
const int Height = 700;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Collider Click Sample",
    Width = Width,
    Height = Height,
    VSync = true
});

// Camera + ClickHandler are created first so the handler's Update — which
// publishes the current mouse world position — runs before any rectangle
// reads it for its hover check.
GameObject.Instantiate("Camera").AddComponent<Camera>();

var clickHandler = GameObject.Instantiate("ClickHandler")
    .AddComponent<ClickHandlerComponent>();
clickHandler.ScreenWidth = Width;
clickHandler.ScreenHeight = Height;

AddBackground(new Vector2(Width / 2f, Height / 2f), new Vector2(Width, Height),
    Color.FromArgb(255, 24, 28, 36));

AddClickableRectangle("Red", new Vector2(220, 200), new Vector2(160, 110), Color.IndianRed);
AddClickableRectangle("Green", new Vector2(500, 200), new Vector2(180, 160), Color.SeaGreen);
AddClickableRectangle("Blue", new Vector2(800, 200), new Vector2(140, 200), Color.SteelBlue);
AddClickableRectangle("Orange", new Vector2(300, 500), new Vector2(220, 120), Color.DarkOrange);
AddClickableRectangle("Purple", new Vector2(720, 500), new Vector2(170, 170), Color.MediumPurple);

window.Run();

static void AddBackground(Vector2 position, Vector2 size, Color color)
{
    var go = GameObject.Instantiate("Background");
    go.Transform.Position = position;
    var renderer = go.AddComponent<ShapeRenderer>();
    renderer.Size = size;
    renderer.Color = color;
    renderer.ZIndex = -1f;
}

static void AddClickableRectangle(string name, Vector2 position, Vector2 size, Color normal)
{
    var go = GameObject.Instantiate(name);
    go.Transform.Position = position;

    var renderer = go.AddComponent<ShapeRenderer>();
    renderer.Size = size;
    renderer.Color = normal;

    var collider = go.AddComponent<Collider>();
    collider.Shape = new RectangleCollisionShape { Size = size };
    collider.Anchor = new Vector2(0.5f, 0.5f);

    var clickable = go.AddComponent<ClickableRectangleComponent>();
    clickable.NormalColor = normal;
}