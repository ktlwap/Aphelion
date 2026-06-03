using System.Drawing;
using System.Numerics;
using Aphelion.Components;
using Aphelion.Core;

namespace ColliderClickSample;

internal sealed class ClickableRectangleComponent : BaseComponent
{
    private const float HoverScale = 1.15f;
    private const float TransitionSpeed = 12f;

    public Color NormalColor { get; set; } = Color.White;

    private static readonly Random Random = new();

    private ShapeRenderer _renderer = null!;
    private Collider _collider = null!;
    private Vector2 _baseSize;
    private float _currentScale = 1f;
    private DateTime _lastUpdate;

    public override void Start()
    {
        _renderer = GameObject.GetComponent<ShapeRenderer>();
        _collider = GameObject.GetComponent<Collider>();
        _baseSize = _renderer.Size;
        _renderer.Color = NormalColor;
        _lastUpdate = DateTime.UtcNow;
    }

    public override void Update()
    {
        var now = DateTime.UtcNow;
        var dt = (float)(now - _lastUpdate).TotalSeconds;
        _lastUpdate = now;

        var isHovered = _collider.IsWithInCollider(ClickHandlerComponent.MouseWorldPosition);
        var target = isHovered ? HoverScale : 1f;

        var t = 1f - MathF.Exp(-TransitionSpeed * dt);
        _currentScale += (target - _currentScale) * t;
        _renderer.Size = _baseSize * _currentScale;
    }

    public void OnClick()
    {
        // Rotate the hue by 90–270° from the current color so the new color is
        // always at least a quarter of the color wheel away — never visually
        // close to what was already on screen.
        var hueOffset = (float)(Random.NextDouble() * 180.0 + 90.0);
        var newHue = (_renderer.Color.GetHue() + hueOffset) % 360f;
        _renderer.Color = HsvToRgb(newHue, 0.85f, 0.95f);
    }

    private static Color HsvToRgb(float hue, float saturation, float value)
    {
        var c = value * saturation;
        var hh = hue / 60f;
        var x = c * (1f - MathF.Abs((hh % 2f) - 1f));
        var m = value - c;

        float r, g, b;
        if (hh < 1f)
        {
            r = c;
            g = x;
            b = 0f;
        }
        else if (hh < 2f)
        {
            r = x;
            g = c;
            b = 0f;
        }
        else if (hh < 3f)
        {
            r = 0f;
            g = c;
            b = x;
        }
        else if (hh < 4f)
        {
            r = 0f;
            g = x;
            b = c;
        }
        else if (hh < 5f)
        {
            r = x;
            g = 0f;
            b = c;
        }
        else
        {
            r = c;
            g = 0f;
            b = x;
        }

        return Color.FromArgb(
            255,
            (int)((r + m) * 255f),
            (int)((g + m) * 255f),
            (int)((b + m) * 255f));
    }
}