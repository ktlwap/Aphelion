using System.Numerics;

namespace Aphelion.Physics;

public sealed class RectangleCollisionShape : CollisionShape
{
    public Vector2 Size { get; set; }

    public override bool CollidesWith(Vector2 position, CollisionShape other, Vector2 otherPosition)
    {
        return other switch
        {
            CircleCollisionShape circle => CollisionDetector.CircleToRectangle(otherPosition, circle.Radius, circle.Anchor, position, Size, Anchor),
            RectangleCollisionShape rect => CollisionDetector.RectangleToRectangle(position, Size, Anchor, otherPosition, rect.Size, rect.Anchor),
            _ => false
        };
    }

    public override bool Contains(Vector2 position, Vector2 point)
    {
        var left = position.X - Size.X * Anchor.X;
        var right = left + Size.X;
        var top = position.Y - Size.Y * Anchor.Y;
        var bottom = top + Size.Y;

        return point.X >= left && point.X <= right && point.Y >= top && point.Y <= bottom;
    }
}
