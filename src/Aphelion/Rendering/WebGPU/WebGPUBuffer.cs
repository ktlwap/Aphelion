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
    private readonly Buffer* _pBuffer;
    private readonly BufferType _bufferType;
    private readonly uint _size;
    
    public BufferType BufferType => _bufferType;
    
    public uint Size => _size;
    
    internal static WebGPUBuffer<TDataType> Upload(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, BufferType type, TDataType* pData, uint dataLength)
    {
        var bufferDescriptor = new BufferDescriptor
        {
            Size = (ulong)(dataLength * sizeof(TDataType)),
            Usage = GetBufferUsageFromType(type)
        };
        
        var pBuffer = webGpu.DeviceCreateBuffer(context.Device, &bufferDescriptor);
        
        var pBufferRange = (byte*)webGpu.BufferGetMappedRange(pBuffer, 0, (nuint)bufferDescriptor.Size);
        
        System.Buffer.MemoryCopy(pData, pBufferRange, dataLength, dataLength);
        
        webGpu.BufferUnmap(pBuffer);
        
        return new WebGPUBuffer<TDataType>(webGpu, pBuffer, type, dataLength);
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
    
    private WebGPUBuffer(Silk.NET.WebGPU.WebGPU webGpu, Buffer* pBuffer, BufferType bufferType, uint size)
    {
        _webGpu = webGpu;
        _pBuffer = pBuffer;
        _bufferType = bufferType;
        _size = size;
    }
    
    public void Dispose()
    {
        _webGpu.BufferRelease(_pBuffer);
    }
}