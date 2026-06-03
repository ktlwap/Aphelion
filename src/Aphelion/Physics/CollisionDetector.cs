using System.Numerics;

namespace Aphelion.Physics;

public static class CollisionDetector
{
    public static bool CircleToCircle(Vector2 p1, float r1, Vector2 a1, Vector2 p2, float r2, Vector2 a2)
    {
        var o1 = new Vector2(r1 * 2 * (a1.X - 0.5f), r1 * 2 * (a1.Y - 0.5f));
        var o2 = new Vector2(r2 * 2 * (a2.X - 0.5f), r2 * 2 * (a2.Y - 0.5f));
        var distanceSquared = Vector2.DistanceSquared(p1 - o1, p2 - o2);
        var radiusSum = r1 + r2;
        return distanceSquared <= radiusSum * radiusSum;
    }

    public static bool RectangleToRectangle(Vector2 p1, Vector2 s1, Vector2 a1, Vector2 p2, Vector2 s2, Vector2 a2)
    {
        var l1 = p1.X - s1.X * a1.X;
        var r1 = l1 + s1.X;
        var t1 = p1.Y - s1.Y * a1.Y;
        var b1 = t1 + s1.Y;

        var l2 = p2.X - s2.X * a2.X;
        var r2 = l2 + s2.X;
        var t2 = p2.Y - s2.Y * a2.Y;
        var b2 = t2 + s2.Y;

        return l1 < r2 && r1 > l2 && t1 < b2 && b1 > t2;
    }

    public static bool CircleToRectangle(Vector2 circlePos, float radius, Vector2 circleAnchor, Vector2 rectPos, Vector2 rectSize, Vector2 rectAnchor)
    {
        var circleOffset = new Vector2(radius * 2 * (circleAnchor.X - 0.5f), radius * 2 * (circleAnchor.Y - 0.5f));
        var actualCirclePos = circlePos - circleOffset;

        var rectLeft = rectPos.X - rectSize.X * rectAnchor.X;
        var rectRight = rectLeft + rectSize.X;
        var rectTop = rectPos.Y - rectSize.Y * rectAnchor.Y;
        var rectBottom = rectTop + rectSize.Y;

        var closestX = Math.Clamp(actualCirclePos.X, rectLeft, rectRight);
        var closestY = Math.Clamp(actualCirclePos.Y, rectTop, rectBottom);

        var distanceSquared = Vector2.DistanceSquared(actualCirclePos, new Vector2(closestX, closestY));
        return distanceSquared <= radius * radius;
    }
}
