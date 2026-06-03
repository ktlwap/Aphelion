using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace WorldUiRenderSample;

internal sealed class CameraControllerComponent : BaseComponent
{
    public float PanAmplitude { get; set; } = 200f;
    public float PanSpeed { get; set; } = 0.6f;
    public float ZoomAmplitude { get; set; } = 0.15f;
    public float ZoomSpeed { get; set; } = 0.4f;

    private readonly DateTime _start = DateTime.UtcNow;

    public override void Update()
    {
        float t = (float)(DateTime.UtcNow - _start).TotalSeconds;

        Camera.Main?.Transform.Position = new Vector2(
            MathF.Sin(t * PanSpeed * MathF.Tau) * PanAmplitude,
            MathF.Cos(t * PanSpeed * MathF.Tau * 0.5f) * PanAmplitude * 0.3f);

        Camera.Main?.Zoom = 1f + MathF.Sin(t * ZoomSpeed * MathF.Tau) * ZoomAmplitude;
    }
}
