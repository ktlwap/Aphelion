using Aphelion.Rendering;
using WorldUiRenderSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "World + UI Render Sample",
    Width = WorldUiRenderScene.ScreenWidth,
    Height = WorldUiRenderScene.ScreenHeight
});

window.Run<WorldUiRenderScene>();
