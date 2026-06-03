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

    private readonly Dictionary<char, Glyph> _glyphs;
    private readonly WebGPUTexture _atlas;

    internal WebGPUTexture Atlas => _atlas;
    internal float BakedSize { get; }
    internal float Ascent { get; }
    internal float Descent { get; }
    internal float LineGap { get; }

    internal static WebGPUFont Load(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, string path, float bakedSize = 48f)
    {
        byte[] fontBytes = File.ReadAllBytes(path);
        var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
        try
        {
            byte* pFontData = (byte*)handle.AddrOfPinnedObject();
            var info = new StbTrueType.stbtt_fontinfo();
            if (StbTrueType.stbtt_InitFont(info, pFontData, 0) == 0)
                throw new InvalidOperationException($"Failed to initialize font: {path}");

            float scale = StbTrueType.stbtt_ScaleForPixelHeight(info, bakedSize);

            int ascent, descent, lineGap;
            StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);

            byte[] atlasData = new byte[AtlasWidth * AtlasHeight * 4];
            var glyphs = new Dictionary<char, Glyph>();

            int penX = GlyphPadding;
            int penY = GlyphPadding;
            int rowH = 0;

            for (char c = (char)32; c <= (char)126; c++)
            {
                int advance, lsb;
                StbTrueType.stbtt_GetCodepointHMetrics(info, c, &advance, &lsb);

                int w, h, xoff, yoff;
                byte* sdfData = StbTrueType.stbtt_GetCodepointSDF(info, scale, c, GlyphPadding, OnEdgeValue, PixelDistScale, &w, &h, &xoff, &yoff);

                if (sdfData != null && w > 0 && h > 0)
                {
                    if (penX + w + GlyphPadding >= AtlasWidth)
                    {
                        penX = GlyphPadding;
                        penY += rowH + GlyphPadding;
                        rowH = 0;
                    }

                    if (penY + h + GlyphPadding >= AtlasHeight)
                    {
                        StbTrueType.stbtt_FreeSDF(sdfData, null);
                        break;
                    }

                    for (int yy = 0; yy < h; yy++)
                    {
                        for (int xx = 0; xx < w; xx++)
                        {
                            byte v = sdfData[yy * w + xx];
                            int idx = ((penY + yy) * AtlasWidth + (penX + xx)) * 4;
                            atlasData[idx + 0] = v;
                            atlasData[idx + 1] = v;
                            atlasData[idx + 2] = v;
                            atlasData[idx + 3] = v;
                        }
                    }

                    glyphs[c] = new Glyph
                    {
                        Uv0 = new Vector2(penX / (float)AtlasWidth, penY / (float)AtlasHeight),
                        Uv1 = new Vector2((penX + w) / (float)AtlasWidth, (penY + h) / (float)AtlasHeight),
                        Width = w,
                        Height = h,
                        OffsetX = xoff,
                        OffsetY = yoff,
                        Advance = advance * scale,
                    };

                    penX += w + GlyphPadding;
                    if (h > rowH) rowH = h;

                    StbTrueType.stbtt_FreeSDF(sdfData, null);
                }
                else
                {
                    glyphs[c] = new Glyph
                    {
                        Uv0 = Vector2.Zero,
                        Uv1 = Vector2.Zero,
                        Width = 0,
                        Height = 0,
                        OffsetX = 0,
                        OffsetY = 0,
                        Advance = advance * scale,
                    };
                }
            }

            WebGPUTexture atlas;
            fixed (byte* pAtlasData = atlasData)
            {
                atlas = WebGPUTexture.Upload(webGpu, context, AtlasWidth, AtlasHeight, pAtlasData, (uint)atlasData.Length);
            }

            return new WebGPUFont(atlas, bakedSize, ascent * scale, descent * scale, lineGap * scale, glyphs);
        }
        finally
        {
            handle.Free();
        }
    }

    internal bool TryGetGlyph(char c, out Glyph glyph) => _glyphs.TryGetValue(c, out glyph);

    private WebGPUFont(WebGPUTexture atlas, float bakedSize, float ascent, float descent, float lineGap, Dictionary<char, Glyph> glyphs)
    {
        _atlas = atlas;
        BakedSize = bakedSize;
        Ascent = ascent;
        Descent = descent;
        LineGap = lineGap;
        _glyphs = glyphs;
    }

    public void Dispose()
    {
        _atlas.Dispose();
    }
}
