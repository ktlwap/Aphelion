using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;
using QuadRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Quads Sample",
    Width = 800,
    Height = 600,
    VSync = true
});

// Dark gray background filling the whole window
var bg = GameObject.Instantiate("Background");
var bgComp = bg.AddComponent<QuadComponent>();
bgComp.Color = Color.FromArgb(255, 30, 30, 30);
bgComp.Size = new Vector2(800, 600);
bgComp.ZIndex = -1f;
bg.Transform.Position = new Vector2(400, 300);

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
spinner.Transform.Position = new Vector2(400, 300);

window.Run();