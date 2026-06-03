using Silk.NET.WebGPU;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPUShader : IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    private readonly ShaderModule* _pModule;

    internal WebGPUShader(Silk.NET.WebGPU.WebGPU webGpu, ShaderModule* pModule)
    {
        _webGpu = webGpu;
        _pModule = pModule;
    }

    public void Dispose()
    {
        _webGpu.ShaderModuleRelease(_pModule);
    }
}