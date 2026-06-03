using System.Drawing;
using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;
using TextRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Text Render Sample",
    Width = 1000,
    Height = 700,
    VSync = true,
});

const string Roboto = "Assets/Fonts/Roboto/Roboto-VariableFont_wdth,wght.ttf";
const string BlackOps = "Assets/Fonts/Black_Ops_One/BlackOpsOne-Regular.ttf";
const string Playwrite = "Assets/Fonts/Playwrite_AU_VIC_Guides/PlaywriteAUVICGuides-Regular.ttf";

AddText("Title", new Vector2(40, 60), "Aphelion", BlackOps, 72f, Color.White);
AddText("Subtitle", new Vector2(40, 130), "SDF text rendering demo", Roboto, 24f, Color.LightGray);

AddText("Roboto18", new Vector2(40, 210), "Roboto 18px — the quick brown fox jumps over the lazy dog", Roboto, 18f,
    Color.PaleTurquoise);
AddText("Roboto28", new Vector2(40, 260), "Roboto 28px — the quick brown fox", Roboto, 28f, Color.PaleTurquoise);
AddText("Roboto42", new Vector2(40, 320), "Roboto 42px — Hello", Roboto, 42f, Color.PaleTurquoise);
AddText("Roboto64", new Vector2(40, 390), "Roboto 64px", Roboto, 64f, Color.PaleTurquoise);

AddText("BlackOps24", new Vector2(40, 490), "Black Ops 24px — MISSION BRIEFING", BlackOps, 24f, Color.OrangeRed);
AddText("BlackOps48", new Vector2(40, 540), "BLACK OPS 48px", BlackOps, 48f, Color.OrangeRed);

AddText("Script32", new Vector2(40, 620), "Playwrite 32px — Greetings!", Playwrite, 32f, Color.Pink);

window.Run();

static void AddText(string name, Vector2 position, string text, string fontPath, float fontSize, Color color)
{
    var go = GameObject.Instantiate(name);
    var c = go.AddComponent<TextComponent>();
    c.Text = text;
    c.FontPath = fontPath;
    c.FontSize = fontSize;
    c.Color = color;
    go.Transform.Position = position;
}