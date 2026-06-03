using System.Drawing;
using System.Numerics;

namespace Aphelion.Rendering;

public abstract class DrawCommand
{
    public required Vector2 Position { get; init; }
    public required float Rotation { get; init; }
    public required float ZIndex { get; init; }
    public required Color Color { get; init; }
}

public sealed class DrawShapeCommand : DrawCommand
{
    public required Vector2 Size { get; init; }
}

public sealed class DrawTextureCommand : DrawCommand
{
    public required Texture Texture { get; init; }
    public required Vector2 Size { get; init; }
}

public sealed class DrawTextCommand : DrawCommand
{
    public Font? Font { get; init; }
    public required string Text { get; init; }
    public required float FontSize { get; init; }
}
