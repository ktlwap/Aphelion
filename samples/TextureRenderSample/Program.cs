using Aphelion.Rendering;
using TextureRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Texture Render Sample",
    Width = TextureRenderScene.ScreenWidth,
    Height = TextureRenderScene.ScreenHeight,
    VSync = true,
});

window.Run<TextureRenderScene>();
