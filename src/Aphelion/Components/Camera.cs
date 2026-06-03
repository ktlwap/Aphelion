using System.Numerics;
using Aphelion.Core;

namespace Aphelion.Components;

public class Camera : BaseComponent
{
    public static Camera? Main { get; private set; }
    
    public float Zoom { get; set; } = 1f;

    public Camera()
    {
        Main ??= this;
    }
    
    public Matrix4x4 GetViewMatrix(int screenWidth, int screenHeight)
    {
        var cx = screenWidth * 0.5f;
        var cy = screenHeight * 0.5f;
        
        var position = Transform.Position;
        
        return Matrix4x4.CreateTranslation(-cx - position.X, -cy - position.Y, 0f)
               * Matrix4x4.CreateRotationZ(-Transform.Rotation)
               * Matrix4x4.CreateScale(Zoom, Zoom, 1f)
               * Matrix4x4.CreateTranslation(cx, cy, 0f);
    }
}
