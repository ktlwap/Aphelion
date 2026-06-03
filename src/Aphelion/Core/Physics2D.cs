using System.Numerics;
using Aphelion.Components;
using Aphelion.Physics;

namespace Aphelion.Core;

public static class Physics2D
{
    public static RayCastHit? RayCast(Vector2 worldPosition)
    {
        foreach (var collider in Collider.Colliders)
        {
            if (collider.IsWithInCollider(worldPosition))
                return new RayCastHit()
                {
                    Collider = collider
                };
        }

        return null;
    }
}
