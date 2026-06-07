using AnimatedSpriteSample;
using Aphelion.Rendering;

var window = Window.Create(new WindowCreationOptions
{
    Title = "Animated Sprite Sample",
    Width = AnimatedSpriteScene.ScreenWidth,
    Height = AnimatedSpriteScene.ScreenHeight,
    VSync = true,
});

window.Run<AnimatedSpriteScene>();
