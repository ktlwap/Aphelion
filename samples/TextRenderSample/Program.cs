using Aphelion.Rendering;
using TextRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Text Render Sample",
    Width = TextRenderScene.ScreenWidth,
    Height = TextRenderScene.ScreenHeight,
    VSync = true,
});

window.Run<TextRenderScene>();
