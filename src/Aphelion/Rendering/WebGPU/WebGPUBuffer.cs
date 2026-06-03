using Silk.NET.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace Aphelion.Rendering.WebGPU;

internal enum BufferType
{
    Vertex,
    Index,
}

internal unsafe class WebGPUBuffer<TDataType> : IDisposable
    where TDataType : unmanaged
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    
    internal required Buffer* PBuffer { get; init; }
    internal required BufferType BufferType { get; init; }
    internal required uint Size { get; init; }
    internal required ulong ByteSize { get; init; }

    internal static WebGPUBuffer<TDataType> Upload(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, BufferType type, TDataType* pData, uint dataLength)
    {
        var bufferSize = (ulong)(dataLength * sizeof(TDataType));
        var bufferDescriptor = new BufferDescriptor
        {
            Size = bufferSize,
            Usage = GetBufferUsageFromType(type),
            MappedAtCreation = true
        };

        var pBuffer = webGpu.DeviceCreateBuffer(context.Device, &bufferDescriptor);

        var pBufferRange = (byte*)webGpu.BufferGetMappedRange(pBuffer, 0, (nuint)bufferSize);
        System.Buffer.MemoryCopy(pData, pBufferRange, (long)bufferSize, (long)bufferSize);
        webGpu.BufferUnmap(pBuffer);

        return new WebGPUBuffer<TDataType>(webGpu)
        {
            PBuffer = pBuffer,
            BufferType = type,
            Size = dataLength,
            ByteSize = bufferSize
        };
    }

    private static BufferUsage GetBufferUsageFromType(BufferType type)
    {
        return type switch
        {
            BufferType.Vertex => BufferUsage.Vertex | BufferUsage.CopyDst,
            BufferType.Index => BufferUsage.Index | BufferUsage.CopyDst,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private WebGPUBuffer(Silk.NET.WebGPU.WebGPU webGpu)
    {
        _webGpu = webGpu;
    }

    public void Dispose()
    {
        _webGpu.BufferRelease(PBuffer);
    }
}
