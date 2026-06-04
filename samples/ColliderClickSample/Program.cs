using Aphelion.Rendering;
using ColliderClickSample;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Collider Click Sample",
    Width = ColliderClickScene.ScreenWidth,
    Height = ColliderClickScene.ScreenHeight,
    VSync = true
});

window.Run<ColliderClickScene>();
