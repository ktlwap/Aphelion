using System.Numerics;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using DrawingColor = System.Drawing.Color;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPUContext : IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    private readonly Instance* _pInstance;
    private readonly Surface* _pSurface;
    private readonly Adapter* _pAdapter;
    private WebGPURenderPipeline? _pipeline;
    private WebGPUUniformBuffer? _uniformBuffer;
    private Sampler* _sampler;
    private WebGPUTexture? _defaultTexture;
    private readonly int _width;
    private readonly int _height;

    internal Device* Device { get; }
    internal Queue* Queue { get; }
    internal TextureFormat SwapChainFormat { get; }

    internal static WebGPUContext Create(IWindow window)
    {
        var webGpu = Silk.NET.WebGPU.WebGPU.GetApi();

        Instance* pInstance = webGpu.CreateInstance(null);
        var pSurface = window.CreateWebGPUSurface(webGpu, pInstance);

        var requestAdapterOptions = new RequestAdapterOptions
        {
            CompatibleSurface = pSurface,
            PowerPreference = PowerPreference.HighPerformance
        };

        Adapter* pAdapter;
        webGpu.InstanceRequestAdapter(pInstance, &requestAdapterOptions, PfnRequestAdapterCallback.From(
            (status, adapter, message, userData) =>
            {
                if (status == RequestAdapterStatus.Success)
                    *(Adapter**)userData = adapter;
            }), &pAdapter);

        var deviceDescriptor = new DeviceDescriptor();
        Device* pDevice;
        webGpu.AdapterRequestDevice(pAdapter, &deviceDescriptor, PfnRequestDeviceCallback.From(
            (status, device, message, userData) =>
            {
                if (status == RequestDeviceStatus.Success)
                    *(Device**)userData = device;
            }), &pDevice);

        var queue = webGpu.DeviceGetQueue(pDevice);
        var swapChainFormat = webGpu.SurfaceGetPreferredFormat(pSurface, pAdapter);

        var surfaceConfiguration = new SurfaceConfiguration
        {
            Device = pDevice,
            Format = swapChainFormat,
            Usage = TextureUsage.RenderAttachment,
            Width = (uint)window.Size.X,
            Height = (uint)window.Size.Y,
            PresentMode = PresentMode.Fifo
        };
        webGpu.SurfaceConfigure(pSurface, &surfaceConfiguration);

        return new WebGPUContext(webGpu, pInstance, pSurface, pAdapter, pDevice, queue, swapChainFormat, window.Size.X, window.Size.Y);
    }

    private WebGPUContext(Silk.NET.WebGPU.WebGPU webGpu, Instance* pInstance, Surface* pSurface, Adapter* pAdapter, Device* pDevice, Queue* pQueue, TextureFormat swapChainFormat, int width, int height)
    {
        _webGpu = webGpu;
        _pInstance = pInstance;
        _pSurface = pSurface;
        _pAdapter = pAdapter;
        Device = pDevice;
        Queue = pQueue;
        SwapChainFormat = swapChainFormat;
        _width = width;
        _height = height;
        
        RenderAssetManager.Initialize(webGpu, this);
    }

    internal void Setup(string shaderSource)
    {
        var attributes = stackalloc VertexAttribute[8];
        // Per-vertex
        attributes[0] = new VertexAttribute { ShaderLocation = 0, Offset = 0,  Format = VertexFormat.Float32x2 }; // Position
        attributes[1] = new VertexAttribute { ShaderLocation = 1, Offset = 8,  Format = VertexFormat.Float32x2 }; // Uv
        // Per-instance (duplicated per vertex since we use a single vertex buffer)
        attributes[2] = new VertexAttribute { ShaderLocation = 2, Offset = 16, Format = VertexFormat.Float32x2 }; // InstancePosition
        attributes[3] = new VertexAttribute { ShaderLocation = 3, Offset = 24, Format = VertexFormat.Float32x2 }; // Scale
        attributes[4] = new VertexAttribute { ShaderLocation = 4, Offset = 32, Format = VertexFormat.Float32 }; // Rotation
        attributes[5] = new VertexAttribute { ShaderLocation = 5, Offset = 36, Format = VertexFormat.Float32 }; // ZIndex
        attributes[6] = new VertexAttribute { ShaderLocation = 6, Offset = 40, Format = VertexFormat.Float32x4 }; // Color
        attributes[7] = new VertexAttribute { ShaderLocation = 7, Offset = 56, Format = VertexFormat.Float32 }; // IsSdf

        var stride = (uint)sizeof(Vertex); // 60 bytes

        _uniformBuffer = WebGPUUniformBuffer.Allocate(_webGpu, this, 64); // mat4x4<f32>
        _pipeline = WebGPURenderPipeline.Compile(_webGpu, this, _uniformBuffer, shaderSource, attributes, 8, stride);

        var samplerDesc = new SamplerDescriptor
        {
            AddressModeU = AddressMode.ClampToEdge,
            AddressModeV = AddressMode.ClampToEdge,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MaxAnisotropy = 1
        };
        _sampler = _webGpu.DeviceCreateSampler(Device, &samplerDesc);

        // Default 1×1 white texture used for DrawShapeCommand
        byte* white = stackalloc byte[] { 255, 255, 255, 255 };
        _defaultTexture = WebGPUTexture.Upload(_webGpu, this, 1, 1, white, 4);
    }

    internal DrawCommandBuffer CreateCommandBuffer() => new DrawCommandBuffer();

    internal void QueueCommandBuffer(DrawCommandBuffer commandBuffer)
    {
        SurfaceTexture surfaceTexture;
        _webGpu.SurfaceGetCurrentTexture(_pSurface, &surfaceTexture);

        var viewDesc = new TextureViewDescriptor
        {
            Format = SwapChainFormat,
            Dimension = TextureViewDimension.Dimension2D,
            Aspect = TextureAspect.All,
            MipLevelCount = 1,
            ArrayLayerCount = 1
        };
        var pTargetView = _webGpu.TextureCreateView(surfaceTexture.Texture, &viewDesc);

        var encoderDesc = new CommandEncoderDescriptor();
        var pEncoder = _webGpu.DeviceCreateCommandEncoder(Device, &encoderDesc);

        var colorAttachment = new RenderPassColorAttachment
        {
            View = pTargetView,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color(0, 0, 0, 1)
        };

        var renderPassDesc = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment
        };

        var pRenderPass = _webGpu.CommandEncoderBeginRenderPass(pEncoder, &renderPassDesc);

        WebGPUBuffer<Vertex>? vertexBuffer = null;
        WebGPUBuffer<ushort>? indexBuffer = null;
        List<nint> bindGroups = new();

        var commands = commandBuffer.DrawCommands;
        if (commands.Count > 0 && _pipeline != null)
        {
            DrawScene(pRenderPass, commands, out vertexBuffer, out indexBuffer, bindGroups);
        }

        _webGpu.RenderPassEncoderEnd(pRenderPass);
        _webGpu.RenderPassEncoderRelease(pRenderPass);

        var commandBufferDesc = new CommandBufferDescriptor();
        var pWgpuCommandBuffer = _webGpu.CommandEncoderFinish(pEncoder, &commandBufferDesc);
        _webGpu.QueueSubmit(Queue, 1, &pWgpuCommandBuffer);

        _webGpu.SurfacePresent(_pSurface);

        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();
        foreach (var bgInt in bindGroups)
            _webGpu.BindGroupRelease((BindGroup*)bgInt);

        _webGpu.CommandBufferRelease(pWgpuCommandBuffer);
        _webGpu.CommandEncoderRelease(pEncoder);
        _webGpu.TextureViewRelease(pTargetView);
        _webGpu.TextureRelease(surfaceTexture.Texture);
    }

    private struct QuadDraw
    {
        public Vector2 Position;
        public Vector2 Size;
        public float Rotation;
        public float ZIndex;
        public Vector4 Color;
        public Vector2 Uv0;
        public Vector2 Uv1;
        public WebGPUTexture WebGPUTexture;
        public float IsSdf;
    }

    private void DrawScene(RenderPassEncoder* pRenderPass, IReadOnlyList<DrawCommand> commands, out WebGPUBuffer<Vertex> vertexBuffer, out WebGPUBuffer<ushort> indexBuffer, List<nint> bindGroups)
    {
        // Orthographic projection: screen pixels → NDC, y-axis flipped
        float w = _width, h = _height;
        var projection = new Matrix4x4(
            2f / w,   0,       0, 0,
            0,       -2f / h,  0, 0,
            0,        0,       1, 0,
           -1f,       1f,      0, 1
        );
        _uniformBuffer!.Write(Queue, projection);

        var uniformBg = _pipeline!.CreateUniformBindGroup(Device, _uniformBuffer.GpuBuffer, _uniformBuffer.Size);

        // Sort back-to-front, then flatten each command into 1+ quads (text -> one quad per glyph).
        var quads = new List<QuadDraw>();
        foreach (var cmd in commands.OrderBy(c => c.ZIndex))
        {
            switch (cmd)
            {
                case DrawShapeCommand shape:
                    quads.Add(new QuadDraw
                    {
                        Position = shape.Position,
                        Size = shape.Size,
                        Rotation = shape.Rotation,
                        ZIndex = shape.ZIndex,
                        Color = ToVec4(shape.Color),
                        Uv0 = Vector2.Zero,
                        Uv1 = Vector2.One,
                        WebGPUTexture = _defaultTexture!,
                        IsSdf = 0f,
                    });
                    break;
                case DrawTextureCommand texCmd:
                    var loaded = RenderAssetManager.GetGpuTexture(texCmd.Texture);
                    if (loaded != null)
                    {
                        quads.Add(new QuadDraw
                        {
                            Position = texCmd.Position,
                            Size = texCmd.Size,
                            Rotation = texCmd.Rotation,
                            ZIndex = texCmd.ZIndex,
                            Color = ToVec4(texCmd.Color),
                            Uv0 = Vector2.Zero,
                            Uv1 = Vector2.One,
                            WebGPUTexture = RenderAssetManager.GetGpuTexture(texCmd.Texture),
                            IsSdf = 0f,
                        });
                    }
                    break;
                case DrawTextCommand textCmd:
                    var gpuFont = RenderAssetManager.GetGpuFont(textCmd.Font);
                    // TODO: null
                    AppendTextQuads(quads, textCmd, gpuFont);
                    break;
            }
        }

        if (quads.Count == 0)
        {
            // Nothing to draw, but caller still expects buffers it can dispose; allocate 1-byte buffers.
            byte zero = 0;
            vertexBuffer = WebGPUBuffer<Vertex>.Upload(_webGpu, this, BufferType.Vertex, (Vertex*)&zero, 0);
            indexBuffer  = WebGPUBuffer<ushort>.Upload(_webGpu, this, BufferType.Index,  (ushort*)&zero, 0);
            bindGroups.Add((nint)uniformBg);
            return;
        }

        var vertices = new Vertex[quads.Count * 4];
        var indices = new ushort[quads.Count * 6];

        for (int i = 0; i < quads.Count; i++)
        {
            var q = quads[i];
            int vi = i * 4;
            vertices[vi + 0] = new Vertex { Position = new(-0.5f, -0.5f), Uv = new(q.Uv0.X, q.Uv0.Y), InstancePosition = q.Position, Scale = q.Size, Rotation = q.Rotation, ZIndex = q.ZIndex, Color = q.Color, IsSdf = q.IsSdf };
            vertices[vi + 1] = new Vertex { Position = new( 0.5f, -0.5f), Uv = new(q.Uv1.X, q.Uv0.Y), InstancePosition = q.Position, Scale = q.Size, Rotation = q.Rotation, ZIndex = q.ZIndex, Color = q.Color, IsSdf = q.IsSdf };
            vertices[vi + 2] = new Vertex { Position = new( 0.5f,  0.5f), Uv = new(q.Uv1.X, q.Uv1.Y), InstancePosition = q.Position, Scale = q.Size, Rotation = q.Rotation, ZIndex = q.ZIndex, Color = q.Color, IsSdf = q.IsSdf };
            vertices[vi + 3] = new Vertex { Position = new(-0.5f,  0.5f), Uv = new(q.Uv0.X, q.Uv1.Y), InstancePosition = q.Position, Scale = q.Size, Rotation = q.Rotation, ZIndex = q.ZIndex, Color = q.Color, IsSdf = q.IsSdf };

            int ii = i * 6;
            var b = (ushort)(i * 4);
            indices[ii + 0] = b;
            indices[ii + 1] = (ushort)(b + 1);
            indices[ii + 2] = (ushort)(b + 2);
            indices[ii + 3] = b;
            indices[ii + 4] = (ushort)(b + 2);
            indices[ii + 5] = (ushort)(b + 3);
        }

        fixed (Vertex* pVerts = vertices)
        fixed (ushort* pIdx = indices)
        {
            vertexBuffer = WebGPUBuffer<Vertex>.Upload(_webGpu, this, BufferType.Vertex, pVerts, (uint)vertices.Length);
            indexBuffer  = WebGPUBuffer<ushort>.Upload(_webGpu, this, BufferType.Index,  pIdx,  (uint)indices.Length);
        }

        _webGpu.RenderPassEncoderSetPipeline(pRenderPass, _pipeline.RenderPipeline);
        _webGpu.RenderPassEncoderSetVertexBuffer(pRenderPass, 0, vertexBuffer.PBuffer, 0, vertexBuffer.ByteSize);
        _webGpu.RenderPassEncoderSetIndexBuffer(pRenderPass, indexBuffer.PBuffer, IndexFormat.Uint16, 0, indexBuffer.ByteSize);
        _webGpu.RenderPassEncoderSetBindGroup(pRenderPass, 0, uniformBg, 0, null);

        var textureBgs = new Dictionary<WebGPUTexture, nint>();
        WebGPUTexture? prevTex = null;

        for (int i = 0; i < quads.Count; i++)
        {
            var tex = quads[i].WebGPUTexture;

            if (tex != prevTex)
            {
                if (!textureBgs.TryGetValue(tex, out var bgInt))
                {
                    bgInt = (nint)_pipeline.CreateTextureBindGroup(Device, ((WebGPUTexture)tex).TextureView, _sampler!);
                    textureBgs[tex] = bgInt;
                }
                _webGpu.RenderPassEncoderSetBindGroup(pRenderPass, 1, (BindGroup*)bgInt, 0, null);
                prevTex = tex;
            }

            _webGpu.RenderPassEncoderDrawIndexed(pRenderPass, 6, 1, (uint)(i * 6), 0, 0);
        }

        foreach (var (_, bgInt) in textureBgs)
            bindGroups.Add(bgInt);
        bindGroups.Add((nint)uniformBg);
    }

    private static Vector4 ToVec4(DrawingColor color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    private static void AppendTextQuads(List<QuadDraw> quads, DrawTextCommand cmd, WebGPUFont font)
    {
        float scale = cmd.FontSize / font.BakedSize;
        var color = ToVec4(cmd.Color);
        float cos = MathF.Cos(cmd.Rotation);
        float sin = MathF.Sin(cmd.Rotation);
        float pen = 0f;

        foreach (var c in cmd.Text)
        {
            if (!font.TryGetGlyph(c, out var glyph))
                continue;

            if (glyph.Width > 0 && glyph.Height > 0)
            {
                // Quad center in unrotated text-local space (y down, baseline at 0).
                float localX = pen + (glyph.OffsetX + glyph.Width * 0.5f) * scale;
                float localY = (glyph.OffsetY + glyph.Height * 0.5f) * scale;

                float worldDx = localX * cos - localY * sin;
                float worldDy = localX * sin + localY * cos;

                quads.Add(new QuadDraw
                {
                    Position = cmd.Position + new Vector2(worldDx, worldDy),
                    Size = new Vector2(glyph.Width * scale, glyph.Height * scale),
                    Rotation = cmd.Rotation,
                    ZIndex = cmd.ZIndex,
                    Color = color,
                    Uv0 = glyph.Uv0,
                    Uv1 = glyph.Uv1,
                    WebGPUTexture = font.Atlas,
                    IsSdf = 1f,
                });
            }

            pen += glyph.Advance * scale;
        }
    }

    public void Dispose()
    {
        /*foreach (var font in _fonts.Values)
            font.Dispose();
        _fonts.Clear();
        foreach (var tex in _textures.Values)
            tex.Dispose();
        _textures.Clear();*/
        _defaultTexture?.Dispose();
        if (_sampler != null) _webGpu.SamplerRelease(_sampler);
        _pipeline?.Dispose();
        _uniformBuffer?.Dispose();
        _webGpu.DeviceRelease(Device);
        _webGpu.AdapterRelease(_pAdapter);
        _webGpu.SurfaceRelease(_pSurface);
        _webGpu.InstanceRelease(_pInstance);
        _webGpu.Dispose();
    }
}
