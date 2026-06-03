using System.Numerics;
using Aphelion.Core;
using Aphelion.Physics;

namespace Aphelion.Components;

public sealed class Collider : BaseComponent
{
    internal static List<Collider> Colliders = new();
    
    public CollisionShape? Shape { get; set; }
    public Vector2 Anchor { get; set; }
    public bool IsTrigger { get; set; }

    public override void Start()
    {
        Colliders.Add(this);
    }

    public bool IsCollidingWith(Collider otherCollider)
    {
        if (Shape is null || otherCollider.Shape is null)
            return false;

        var position = Transform.Position;
        Shape.Anchor = Anchor;

        Vector2 otherPosition = otherCollider.Transform.Position;
        otherCollider.Shape.Anchor = otherCollider.Anchor;

        return Shape.CollidesWith(position, otherCollider.Shape, otherPosition);
    }

    public bool IsWithInCollider(Vector2 worldPosition)
    {
        if (Shape is null)
            return false;

        Shape.Anchor = Anchor;
        return Shape.Contains(Transform.Position, worldPosition);
    }

    public override void Stop()
    {
        Colliders.Remove(this);
    }
}