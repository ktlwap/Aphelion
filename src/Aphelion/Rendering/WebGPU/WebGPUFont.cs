using System.Numerics;
using System.Runtime.InteropServices;
using StbTrueTypeSharp;

namespace Aphelion.Rendering.WebGPU;

internal struct Glyph
{
    public Vector2 Uv0;
    public Vector2 Uv1;
    public int Width;
    public int Height;
    public int OffsetX;
    public int OffsetY;
    public float Advance;
}

internal unsafe class WebGPUFont : IDisposable
{
    private const int AtlasWidth = 512;
    private const int AtlasHeight = 512;
    private const int GlyphPadding = 4;
    private const byte OnEdgeValue = 128;
    private const float PixelDistScale = 32f;
    private const char FirstChar = (char)32;
    private const char LastChar = (char)126;

    private readonly Dictionary<char, Glyph> _glyphs;

    internal WebGPUTexture Atlas { get; }
    internal float BakedSize { get; }
    internal float Ascent { get; }
    internal float Descent { get; }
    internal float LineGap { get; }

    private WebGPUFont(WebGPUTexture atlas, float bakedSize, float ascent, float descent, float lineGap, Dictionary<char, Glyph> glyphs)
    {
        Atlas = atlas;
        BakedSize = bakedSize;
        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
        _glyphs = glyphs;
    }

    internal static WebGPUFont Load(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, string path, float bakedSize = 48f)
    {
        var fontData = File.ReadAllBytes(path);
        var fontHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
        try
        {
            var info = InitFontInfo((byte*)fontHandle.AddrOfPinnedObject(), path);
            var scale = StbTrueType.stbtt_ScaleForPixelHeight(info, bakedSize);

            int ascent, descent, lineGap;
            StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

            var atlasPixelData = new byte[AtlasWidth * AtlasHeight * 4];
            var glyphs = BakeAsciiGlyphs(info, scale, atlasPixelData);
            var atlas = UploadAtlas(webGpu, context, atlasPixelData);

            return new WebGPUFont(atlas, bakedSize, ascent * scale, descent * scale, lineGap * scale, glyphs);
        }
        finally
        {
            fontHandle.Free();
        }
    }
    
    private static StbTrueType.stbtt_fontinfo InitFontInfo(byte* pFontData, string path)
    {
        var info = new StbTrueType.stbtt_fontinfo();
        if (StbTrueType.stbtt_InitFont(info, pFontData, 0) == 0)
            throw new InvalidOperationException($"Failed to initialize font: {path}");
        return info;
    }

    private static Dictionary<char, Glyph> BakeAsciiGlyphs(StbTrueType.stbtt_fontinfo info, float scale, byte[] atlasPixels)
    {
        var glyphs = new Dictionary<char, Glyph>();
        var shelf = AtlasShelf.Start();

        for (var c = FirstChar; c <= LastChar; c++)
        {
            if (!TryBakeGlyph(info, scale, c, atlasPixels, ref shelf, out var glyph))
                break; // atlas full — drop the rest
            glyphs[c] = glyph;
        }

        return glyphs;
    }
    
    private static bool TryBakeGlyph(
        StbTrueType.stbtt_fontinfo info,
        float scale,
        char c,
        byte[] atlasPixels,
        ref AtlasShelf shelf,
        out Glyph glyph)
    {
        int advance, lsb;
        StbTrueType.stbtt_GetCodepointHMetrics(info, c, &advance, &lsb);

        int w, h, xoff, yoff;
        byte* sdf = StbTrueType.stbtt_GetCodepointSDF(
            info, scale, c, GlyphPadding, OnEdgeValue, PixelDistScale,
            &w, &h, &xoff, &yoff);

        try
        {
            if (sdf == null || w <= 0 || h <= 0)
            {
                glyph = new Glyph { Advance = advance * scale };
                return true;
            }

            if (!shelf.TryReserve(w, h, out int atlasX, out int atlasY))
            {
                glyph = default;
                return false;
            }

            CopySdfToAtlas(sdf, w, h, atlasX, atlasY, atlasPixels);

            glyph = new Glyph
            {
                Uv0 = new Vector2(atlasX / (float)AtlasWidth, atlasY / (float)AtlasHeight),
                Uv1 = new Vector2((atlasX + w) / (float)AtlasWidth, (atlasY + h) / (float)AtlasHeight),
                Width = w,
                Height = h,
                OffsetX = xoff,
                OffsetY = yoff,
                Advance = advance * scale,
            };
            return true;
        }
        finally
        {
            if (sdf != null)
                StbTrueType.stbtt_FreeSDF(sdf, null);
        }
    }
    
    private static void CopySdfToAtlas(
        byte* sdf, int srcW, int srcH,
        int dstX, int dstY,
        byte[] atlasPixels)
    {
        for (int y = 0; y < srcH; y++)
        {
            int srcRow = y * srcW;
            int dstRowStart = ((dstY + y) * AtlasWidth + dstX) * 4;
            for (int x = 0; x < srcW; x++)
            {
                byte v = sdf[srcRow + x];
                int p = dstRowStart + x * 4;
                atlasPixels[p + 0] = v;
                atlasPixels[p + 1] = v;
                atlasPixels[p + 2] = v;
                atlasPixels[p + 3] = v;
            }
        }
    }

    private static WebGPUTexture UploadAtlas(
        Silk.NET.WebGPU.WebGPU webGpu,
        WebGPUContext context,
        byte[] atlasPixels)
    {
        fixed (byte* pAtlas = atlasPixels)
            return WebGPUTexture.Upload(webGpu, context, AtlasWidth, AtlasHeight, pAtlas, (uint)atlasPixels.Length);
    }
    
    private struct AtlasShelf
    {
        public int CursorX;
        public int CursorY;
        public int RowHeight;

        public static AtlasShelf Start() => new()
        {
            CursorX = GlyphPadding,
            CursorY = GlyphPadding,
            RowHeight = 0,
        };

        public bool TryReserve(int glyphW, int glyphH, out int x, out int y)
        {
            if (CursorX + glyphW + GlyphPadding >= AtlasWidth)
            {
                CursorX = GlyphPadding;
                CursorY += RowHeight + GlyphPadding;
                RowHeight = 0;
            }

            if (CursorY + glyphH + GlyphPadding >= AtlasHeight)
            {
                x = 0; y = 0;
                return false;
            }

            x = CursorX;
            y = CursorY;
            CursorX += glyphW + GlyphPadding;
            if (glyphH > RowHeight)
                RowHeight = glyphH;
            return true;
        }
    }
    
    internal bool TryGetGlyph(char c, out Glyph glyph) => _glyphs.TryGetValue(c, out glyph);

    public void Dispose() => Atlas.Dispose();
}
