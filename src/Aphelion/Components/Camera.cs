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

    public Vector2 ScreenToWorldPosition(Vector2 screenPosition, int screenWidth, int screenHeight)
    {
        var cx = screenWidth * 0.5f;
        var cy = screenHeight * 0.5f;

        var p = (screenPosition - new Vector2(cx, cy)) / Zoom;

        var cos = MathF.Cos(Transform.Rotation);
        var sin = MathF.Sin(Transform.Rotation);
        var rotated = new Vector2(
            p.X * cos - p.Y * sin,
            p.X * sin + p.Y * cos
        );

        return rotated + new Vector2(cx + Transform.Position.X, cy + Transform.Position.Y);
    }
}
