using System.Numerics;
using System.Runtime.InteropServices;

namespace Aphelion.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector2 Position;
    public Vector2 Uv;
    public Vector2 InstancePosition;
    public Vector2 Scale;
    public float Rotation;
    public float ZIndex;
    public Vector4 Color;
    public float IsSdf;
}
