using Aphelion.Rendering.WebGPU;
using StbImageSharp;

namespace Aphelion.Rendering;

public static class RenderAssetManager
{
    private static Silk.NET.WebGPU.WebGPU? _webGpu;
    private static WebGPUContext? _context;

    private static Dictionary<Texture, WebGPUTexture> _textures = new();
    private static Dictionary<Font, WebGPUFont> _fonts = new();

    internal static void Initialize(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context)
    {
        _webGpu = webGpu;
        _context = context;
    }

    public static unsafe Texture LoadTexture(string fileName)
    {
        foreach (var gpuTexture in _textures)
        {
            if (gpuTexture.Key.FileName == fileName)
                return gpuTexture.Key;
        }

        if (!File.Exists(fileName))
            throw new Exception("File not found: " + fileName);

        using var fileStream = File.OpenRead(fileName);

        var image = ImageResult.FromStream(fileStream, ColorComponents.RedGreenBlueAlpha);
        fixed (byte* pData = image.Data)
        {
            var texture = new Texture()
            {
                FileName = fileName,
            };

            _textures[texture] = WebGPUTexture.Upload(_webGpu, _context, (uint)image.Width, (uint)image.Height, pData,
                (uint)image.Data.Length);

            return texture;
        }
    }

    public static Font LoadFont(string fileName)
    {
        foreach (var gpuFont in _fonts)
        {
            if (gpuFont.Key.FileName == fileName)
                return gpuFont.Key;
        }

        if (!File.Exists(fileName))
            throw new Exception("File not found: " + fileName);

        using var fileStream = File.OpenRead(fileName);

        var font = new Font()
        {
            FileName = fileName,
        };

        _fonts[font] = WebGPUFont.Load(_webGpu, _context, fileName);

        return font;
    }

    internal static WebGPUTexture GetGpuTexture(Texture texture)
    {
        return _textures[texture];
    }

    internal static WebGPUFont GetGpuFont(Font font)
    {
        return _fonts[font];
    }
}