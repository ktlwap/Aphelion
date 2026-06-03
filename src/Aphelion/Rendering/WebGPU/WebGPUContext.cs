using Silk.NET.WebGPU;
using Silk.NET.Windowing;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPUContext : IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    
    internal Device* Device { get; }
    internal Queue* Queue { get; }

    internal static WebGPUContext Create(IWindow window)
    {
        var webGpu = Silk.NET.WebGPU.WebGPU.GetApi();
        return new WebGPUContext(webGpu);
    }

    private WebGPUContext(Silk.NET.WebGPU.WebGPU webGpu)
    {
        _webGpu = webGpu;
    }

    internal DrawCommandBuffer CreateCommandBuffer()
    {
        return new DrawCommandBuffer();
    }
    
    internal void QueueCommandBuffer(DrawCommandBuffer commandBuffer)
    {
        // QUEUE & RENDER
    }

    public void Dispose()
    {
        _webGpu.Dispose();
    }
}