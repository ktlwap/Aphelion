using Aphelion.Rendering;
using QuadRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Quads Sample",
    Width = QuadRenderScene.ScreenWidth,
    Height = QuadRenderScene.ScreenHeight,
    VSync = true
});

window.Run<QuadRenderScene>();
