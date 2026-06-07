using System.Numerics;
using Aphelion.Core;
using Aphelion.Rendering;

namespace QuadRenderSample;

internal sealed class QuadComponent : BaseComponent
{
    public Color Color { get; set; } = Color.White;
    public Vector2 Size { get; set; } = new Vector2(100, 100);
    public float ZIndex { get; set; } = 0f;
    public float SpinSpeed { get; set; } = 0f;

    private DateTime _lastUpdate;

    public override void Start()
    {
        _lastUpdate = DateTime.UtcNow;
    }

    public override void Update()
    {
        if (SpinSpeed != 0f)
        {
            var now = DateTime.UtcNow;
            Transform.Rotation += SpinSpeed * (float)(now - _lastUpdate).TotalSeconds;
            _lastUpdate = now;
        }
    }

    public override void Render(DrawCommandBuffer buffer)
    {
        buffer.DrawShape(new DrawShapeCommand
        {
            Position = Transform.Position,
            Rotation = Transform.Rotation,
            ZIndex = ZIndex,
            Color = Color,
            Size = Size
        });
    }
}
