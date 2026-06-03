using Silk.NET.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPUUniformBuffer : IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    private readonly Buffer* _pBuffer;
    private readonly ulong _size;

    public ulong Size => _size;
    internal Buffer* GpuBuffer => _pBuffer;

    internal static WebGPUUniformBuffer Allocate(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, ulong size)
    {
        var bufferDescriptor = new BufferDescriptor
        {
            Size = size,
            Usage = BufferUsage.Uniform | BufferUsage.CopyDst,
        };

        var pBuffer = webGpu.DeviceCreateBuffer(context.Device, &bufferDescriptor);

        return new WebGPUUniformBuffer(webGpu, pBuffer, size);
    }

    private WebGPUUniformBuffer(Silk.NET.WebGPU.WebGPU webGpu, Buffer* pBuffer, ulong size)
    {
        _webGpu = webGpu;
        _pBuffer = pBuffer;
        _size = size;
    }

    internal void Write<T>(Queue* queue, T data) where T : unmanaged
    {
        _webGpu.QueueWriteBuffer(queue, _pBuffer, 0, &data, (nuint)sizeof(T));
    }

    public void Dispose()
    {
        _webGpu.BufferRelease(_pBuffer);
    }
}
