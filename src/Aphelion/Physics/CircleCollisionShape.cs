using System.Numerics;

namespace Aphelion.Physics;

public sealed class CircleCollisionShape : CollisionShape
{
    public float Radius { get; set; }

    public override bool CollidesWith(Vector2 position, CollisionShape other, Vector2 otherPosition)
    {
        return other switch
        {
            CircleCollisionShape circle => CollisionDetector.CircleToCircle(position, Radius, Anchor, otherPosition, circle.Radius, circle.Anchor),
            RectangleCollisionShape rect => CollisionDetector.CircleToRectangle(position, Radius, Anchor, otherPosition, rect.Size, rect.Anchor),
            _ => false
        };
    }

    public override bool Contains(Vector2 position, Vector2 point)
    {
        var offset = new Vector2(Radius * 2 * (Anchor.X - 0.5f), Radius * 2 * (Anchor.Y - 0.5f));
        var actualCenter = position - offset;
        return Vector2.DistanceSquared(actualCenter, point) <= Radius * Radius;
    }
}
