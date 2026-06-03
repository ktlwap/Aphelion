using System.Numerics;

namespace Aphelion.Physics;

public abstract class CollisionShape
{
    public Vector2 Anchor { get; set; }
    public abstract bool CollidesWith(Vector2 position, CollisionShape other, Vector2 otherPosition);
    public abstract bool Contains(Vector2 position, Vector2 point);
}
