using System.Numerics;

namespace Aphelion.Core;

public readonly struct Color(float r, float g, float b, float a = 1f) : IEquatable<Color>
{
    public float R { get; } = r;
    public float G { get; } = g;
    public float B { get; } = b;
    public float A { get; } = a;

    public static Color FromArgb(float alpha, float red, float green, float blue) =>
        new(red, green, blue, alpha);

    public static Color FromArgb(float red, float green, float blue) =>
        new(red, green, blue, 1f);

    public static Color FromArgb(float alpha, Color baseColor) =>
        new(baseColor.R, baseColor.G, baseColor.B, alpha);

    public Vector4 ToVector4() => new(R, G, B, A);

    public int ToArgb()
    {
        int a = Pack(A);
        int r = Pack(R);
        int g = Pack(G);
        int b = Pack(B);
        return (a << 24) | (r << 16) | (g << 8) | b;

        static int Pack(float v) => Math.Clamp((int)MathF.Round(v * 255f), 0, 255);
    }

    public float GetHue()
    {
        if (R == G && G == B)
            return 0f;

        float max = MathF.Max(R, MathF.Max(G, B));
        float min = MathF.Min(R, MathF.Min(G, B));
        float delta = max - min;

        float hue;
        if (max == R)
            hue = (G - B) / delta % 6f;
        else if (max == G)
            hue = (B - R) / delta + 2f;
        else
            hue = (R - G) / delta + 4f;

        hue *= 60f;
        if (hue < 0f)
            hue += 360f;
        return hue;
    }

    public float GetSaturation()
    {
        float max = MathF.Max(R, MathF.Max(G, B));
        float min = MathF.Min(R, MathF.Min(G, B));
        if (max == min)
            return 0f;
        float l = (max + min) * 0.5f;
        return l <= 0.5f ? (max - min) / (max + min) : (max - min) / (2f - max - min);
    }

    public float GetBrightness()
    {
        float max = MathF.Max(R, MathF.Max(G, B));
        float min = MathF.Min(R, MathF.Min(G, B));
        return (max + min) * 0.5f;
    }

    public bool Equals(Color other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }
    
    public override bool Equals(object? obj) => obj is Color c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public static bool operator ==(Color l, Color r) => l.Equals(r);
    public static bool operator !=(Color l, Color r) => !l.Equals(r);

    // Helper used only by the named-color table below — public API stays purely
    // 0–1 float; bytes appear only here as a compact way to spell standard sRGB.
    private static Color Rgb(byte r, byte g, byte b, byte a = 255) =>
        new(r / 255f, g / 255f, b / 255f, a / 255f);

    public static Color Transparent { get; } = Rgb(255, 255, 255, 0);
    public static Color AliceBlue { get; } = Rgb(240, 248, 255);
    public static Color AntiqueWhite { get; } = Rgb(250, 235, 215);
    public static Color Aqua { get; } = Rgb(0, 255, 255);
    public static Color Aquamarine { get; } = Rgb(127, 255, 212);
    public static Color Azure { get; } = Rgb(240, 255, 255);
    public static Color Beige { get; } = Rgb(245, 245, 220);
    public static Color Bisque { get; } = Rgb(255, 228, 196);
    public static Color Black { get; } = Rgb(0, 0, 0);
    public static Color BlanchedAlmond { get; } = Rgb(255, 235, 205);
    public static Color Blue { get; } = Rgb(0, 0, 255);
    public static Color BlueViolet { get; } = Rgb(138, 43, 226);
    public static Color Brown { get; } = Rgb(165, 42, 42);
    public static Color BurlyWood { get; } = Rgb(222, 184, 135);
    public static Color CadetBlue { get; } = Rgb(95, 158, 160);
    public static Color Chartreuse { get; } = Rgb(127, 255, 0);
    public static Color Chocolate { get; } = Rgb(210, 105, 30);
    public static Color Coral { get; } = Rgb(255, 127, 80);
    public static Color CornflowerBlue { get; } = Rgb(100, 149, 237);
    public static Color Cornsilk { get; } = Rgb(255, 248, 220);
    public static Color Crimson { get; } = Rgb(220, 20, 60);
    public static Color Cyan { get; } = Rgb(0, 255, 255);
    public static Color DarkBlue { get; } = Rgb(0, 0, 139);
    public static Color DarkCyan { get; } = Rgb(0, 139, 139);
    public static Color DarkGoldenrod { get; } = Rgb(184, 134, 11);
    public static Color DarkGray { get; } = Rgb(169, 169, 169);
    public static Color DarkGreen { get; } = Rgb(0, 100, 0);
    public static Color DarkKhaki { get; } = Rgb(189, 183, 107);
    public static Color DarkMagenta { get; } = Rgb(139, 0, 139);
    public static Color DarkOliveGreen { get; } = Rgb(85, 107, 47);
    public static Color DarkOrange { get; } = Rgb(255, 140, 0);
    public static Color DarkOrchid { get; } = Rgb(153, 50, 204);
    public static Color DarkRed { get; } = Rgb(139, 0, 0);
    public static Color DarkSalmon { get; } = Rgb(233, 150, 122);
    public static Color DarkSeaGreen { get; } = Rgb(143, 188, 139);
    public static Color DarkSlateBlue { get; } = Rgb(72, 61, 139);
    public static Color DarkSlateGray { get; } = Rgb(47, 79, 79);
    public static Color DarkTurquoise { get; } = Rgb(0, 206, 209);
    public static Color DarkViolet { get; } = Rgb(148, 0, 211);
    public static Color DeepPink { get; } = Rgb(255, 20, 147);
    public static Color DeepSkyBlue { get; } = Rgb(0, 191, 255);
    public static Color DimGray { get; } = Rgb(105, 105, 105);
    public static Color DodgerBlue { get; } = Rgb(30, 144, 255);
    public static Color Firebrick { get; } = Rgb(178, 34, 34);
    public static Color FloralWhite { get; } = Rgb(255, 250, 240);
    public static Color ForestGreen { get; } = Rgb(34, 139, 34);
    public static Color Fuchsia { get; } = Rgb(255, 0, 255);
    public static Color Gainsboro { get; } = Rgb(220, 220, 220);
    public static Color GhostWhite { get; } = Rgb(248, 248, 255);
    public static Color Gold { get; } = Rgb(255, 215, 0);
    public static Color Goldenrod { get; } = Rgb(218, 165, 32);
    public static Color Gray { get; } = Rgb(128, 128, 128);
    public static Color Green { get; } = Rgb(0, 128, 0);
    public static Color GreenYellow { get; } = Rgb(173, 255, 47);
    public static Color Honeydew { get; } = Rgb(240, 255, 240);
    public static Color HotPink { get; } = Rgb(255, 105, 180);
    public static Color IndianRed { get; } = Rgb(205, 92, 92);
    public static Color Indigo { get; } = Rgb(75, 0, 130);
    public static Color Ivory { get; } = Rgb(255, 255, 240);
    public static Color Khaki { get; } = Rgb(240, 230, 140);
    public static Color Lavender { get; } = Rgb(230, 230, 250);
    public static Color LavenderBlush { get; } = Rgb(255, 240, 245);
    public static Color LawnGreen { get; } = Rgb(124, 252, 0);
    public static Color LemonChiffon { get; } = Rgb(255, 250, 205);
    public static Color LightBlue { get; } = Rgb(173, 216, 230);
    public static Color LightCoral { get; } = Rgb(240, 128, 128);
    public static Color LightCyan { get; } = Rgb(224, 255, 255);
    public static Color LightGoldenrodYellow { get; } = Rgb(250, 250, 210);
    public static Color LightGray { get; } = Rgb(211, 211, 211);
    public static Color LightGreen { get; } = Rgb(144, 238, 144);
    public static Color LightPink { get; } = Rgb(255, 182, 193);
    public static Color LightSalmon { get; } = Rgb(255, 160, 122);
    public static Color LightSeaGreen { get; } = Rgb(32, 178, 170);
    public static Color LightSkyBlue { get; } = Rgb(135, 206, 250);
    public static Color LightSlateGray { get; } = Rgb(119, 136, 153);
    public static Color LightSteelBlue { get; } = Rgb(176, 196, 222);
    public static Color LightYellow { get; } = Rgb(255, 255, 224);
    public static Color Lime { get; } = Rgb(0, 255, 0);
    public static Color LimeGreen { get; } = Rgb(50, 205, 50);
    public static Color Linen { get; } = Rgb(250, 240, 230);
    public static Color Magenta { get; } = Rgb(255, 0, 255);
    public static Color Maroon { get; } = Rgb(128, 0, 0);
    public static Color MediumAquamarine { get; } = Rgb(102, 205, 170);
    public static Color MediumBlue { get; } = Rgb(0, 0, 205);
    public static Color MediumOrchid { get; } = Rgb(186, 85, 211);
    public static Color MediumPurple { get; } = Rgb(147, 112, 219);
    public static Color MediumSeaGreen { get; } = Rgb(60, 179, 113);
    public static Color MediumSlateBlue { get; } = Rgb(123, 104, 238);
    public static Color MediumSpringGreen { get; } = Rgb(0, 250, 154);
    public static Color MediumTurquoise { get; } = Rgb(72, 209, 204);
    public static Color MediumVioletRed { get; } = Rgb(199, 21, 133);
    public static Color MidnightBlue { get; } = Rgb(25, 25, 112);
    public static Color MintCream { get; } = Rgb(245, 255, 250);
    public static Color MistyRose { get; } = Rgb(255, 228, 225);
    public static Color Moccasin { get; } = Rgb(255, 228, 181);
    public static Color NavajoWhite { get; } = Rgb(255, 222, 173);
    public static Color Navy { get; } = Rgb(0, 0, 128);
    public static Color OldLace { get; } = Rgb(253, 245, 230);
    public static Color Olive { get; } = Rgb(128, 128, 0);
    public static Color OliveDrab { get; } = Rgb(107, 142, 35);
    public static Color Orange { get; } = Rgb(255, 165, 0);
    public static Color OrangeRed { get; } = Rgb(255, 69, 0);
    public static Color Orchid { get; } = Rgb(218, 112, 214);
    public static Color PaleGoldenrod { get; } = Rgb(238, 232, 170);
    public static Color PaleGreen { get; } = Rgb(152, 251, 152);
    public static Color PaleTurquoise { get; } = Rgb(175, 238, 238);
    public static Color PaleVioletRed { get; } = Rgb(219, 112, 147);
    public static Color PapayaWhip { get; } = Rgb(255, 239, 213);
    public static Color PeachPuff { get; } = Rgb(255, 218, 185);
    public static Color Peru { get; } = Rgb(205, 133, 63);
    public static Color Pink { get; } = Rgb(255, 192, 203);
    public static Color Plum { get; } = Rgb(221, 160, 221);
    public static Color PowderBlue { get; } = Rgb(176, 224, 230);
    public static Color Purple { get; } = Rgb(128, 0, 128);
    public static Color Red { get; } = Rgb(255, 0, 0);
    public static Color RosyBrown { get; } = Rgb(188, 143, 143);
    public static Color RoyalBlue { get; } = Rgb(65, 105, 225);
    public static Color SaddleBrown { get; } = Rgb(139, 69, 19);
    public static Color Salmon { get; } = Rgb(250, 128, 114);
    public static Color SandyBrown { get; } = Rgb(244, 164, 96);
    public static Color SeaGreen { get; } = Rgb(46, 139, 87);
    public static Color SeaShell { get; } = Rgb(255, 245, 238);
    public static Color Sienna { get; } = Rgb(160, 82, 45);
    public static Color Silver { get; } = Rgb(192, 192, 192);
    public static Color SkyBlue { get; } = Rgb(135, 206, 235);
    public static Color SlateBlue { get; } = Rgb(106, 90, 205);
    public static Color SlateGray { get; } = Rgb(112, 128, 144);
    public static Color Snow { get; } = Rgb(255, 250, 250);
    public static Color SpringGreen { get; } = Rgb(0, 255, 127);
    public static Color SteelBlue { get; } = Rgb(70, 130, 180);
    public static Color Tan { get; } = Rgb(210, 180, 140);
    public static Color Teal { get; } = Rgb(0, 128, 128);
    public static Color Thistle { get; } = Rgb(216, 191, 216);
    public static Color Tomato { get; } = Rgb(255, 99, 71);
    public static Color Turquoise { get; } = Rgb(64, 224, 208);
    public static Color Violet { get; } = Rgb(238, 130, 238);
    public static Color Wheat { get; } = Rgb(245, 222, 179);
    public static Color White { get; } = Rgb(255, 255, 255);
    public static Color WhiteSmoke { get; } = Rgb(245, 245, 245);
    public static Color Yellow { get; } = Rgb(255, 255, 0);
    public static Color YellowGreen { get; } = Rgb(154, 205, 50);
}