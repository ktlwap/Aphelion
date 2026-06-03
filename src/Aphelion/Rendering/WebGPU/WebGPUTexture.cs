using Silk.NET.WebGPU;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPUTexture : Texture, IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    private readonly Silk.NET.WebGPU.Texture* _pTexture;
    private readonly TextureView* _pTextureView;
    private readonly uint _width;
    private readonly uint _height;
    
    internal static WebGPUTexture Upload(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, uint width, uint height, byte* pData, uint dataLength)
    {
        var extent = new Extent3D(width, height, 1);
        var descriptor = new TextureDescriptor
        {
            Size = extent,
            Format = TextureFormat.Rgba8Unorm,
            Usage = TextureUsage.TextureBinding | TextureUsage.CopyDst,
            Dimension = TextureDimension.Dimension2D,
            MipLevelCount = 1,
            SampleCount = 1
        };
        
        var pTexture = webGpu.DeviceCreateTexture(context.Device, ref descriptor);
        
        var imageCopyTexture = new ImageCopyTexture { Texture = pTexture };
        var dataLayout = new TextureDataLayout 
        { 
            BytesPerRow = width * 4, 
            RowsPerImage = height 
        };
        webGpu.QueueWriteTexture(context.Queue, &imageCopyTexture, pData, dataLength, &dataLayout, &extent);

        var viewDesc = new TextureViewDescriptor
        {
            Format = TextureFormat.Rgba8Unorm,
            Dimension = TextureViewDimension.Dimension2D,
            Aspect = TextureAspect.All,
            MipLevelCount = 1,
            ArrayLayerCount = 1
        };
        var pTextureView = webGpu.TextureCreateView(pTexture, &viewDesc);

        return new WebGPUTexture(webGpu, pTexture, pTextureView, width, height);
    }
    
    private WebGPUTexture(Silk.NET.WebGPU.WebGPU webGpu, Silk.NET.WebGPU.Texture* pTexture, TextureView* pTextureView, uint width, uint height)
    {
        _webGpu = webGpu;
        _pTexture = pTexture;
        _pTextureView = pTextureView;
        _width = width;
        _height = height;
    }

    public void Dispose()
    {
        _webGpu.TextureViewRelease(_pTextureView);
        _webGpu.TextureRelease(_pTexture);
    }
}