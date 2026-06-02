using System.Numerics;

namespace Aphelion.Core;

public sealed class Transform
{
    public readonly GameObject GameObject;

    internal Transform(GameObject gameObject)
    {
        GameObject = gameObject;
    }

    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public float Rotation { get; set; }
}