using System.Runtime.InteropServices;
using Silk.NET.WebGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPURenderPipeline : IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    private readonly BindGroupLayout* _uniformGroupLayout;
    private readonly BindGroupLayout* _textureGroupLayout;

    internal RenderPipeline* RenderPipeline { get; }

    internal static WebGPURenderPipeline Compile(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, WebGPUUniformBuffer uniformBuffer, string shaderSource, VertexAttribute* pAttributes, uint attributeCount, uint stride)
    {
        var shaderDescriptor = new ShaderModuleDescriptor();
        var wgslDescriptor = new ShaderModuleWGSLDescriptor
        {
            Code = (byte*)Marshal.StringToHGlobalAnsi(shaderSource),
            Chain = new ChainedStruct
            {
                SType = SType.ShaderModuleWgslDescriptor
            }
        };
        
        shaderDescriptor.NextInChain = (ChainedStruct*)&wgslDescriptor;

        var pShaderModule = webGpu.DeviceCreateShaderModule(context.Device, &shaderDescriptor);
        
        Marshal.FreeHGlobal((IntPtr)wgslDescriptor.Code);
        
        BindGroupLayoutEntry uniformLayoutEntry = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
            Buffer = new BufferBindingLayout
            {
                Type = BufferBindingType.Uniform,
                HasDynamicOffset = false,
                MinBindingSize = 64
            }
        };

        var quadLayoutDesc = new BindGroupLayoutDescriptor
        {
            EntryCount = 1,
            Entries = &uniformLayoutEntry
        };
        var uniformGroupLayout = webGpu.DeviceCreateBindGroupLayout(context.Device, &quadLayoutDesc);
        
        var textureEntry = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Fragment,
            Texture = new TextureBindingLayout
            {
                Multisampled = false,
                SampleType = TextureSampleType.Float,
                ViewDimension = TextureViewDimension.Dimension2D
            }
        };
        
        var samplerEntry = new BindGroupLayoutEntry
        {
            Binding = 1,
            Visibility = ShaderStage.Fragment,
            Sampler = new SamplerBindingLayout
            {
                Type = SamplerBindingType.Filtering
            }
        };
        
        var spriteLayoutEntries = stackalloc BindGroupLayoutEntry[2];
        spriteLayoutEntries[0] = textureEntry;
        spriteLayoutEntries[1] = samplerEntry;

        var spriteLayoutDesc = new BindGroupLayoutDescriptor
        {
            EntryCount = 2,
            Entries = spriteLayoutEntries
        };
        var textureGroupLayout = webGpu.DeviceCreateBindGroupLayout(context.Device, &spriteLayoutDesc);

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
        bindGroupLayouts[0] = uniformGroupLayout;
        bindGroupLayouts[1] = textureGroupLayout;

        var pipelineLayoutDesc = new PipelineLayoutDescriptor
        {
            BindGroupLayoutCount = 2,
            BindGroupLayouts = bindGroupLayouts
        };
        
        var pipelineLayout = webGpu.DeviceCreatePipelineLayout(context.Device, &pipelineLayoutDesc);

        var bufferLayout = new VertexBufferLayout
        {
            ArrayStride = stride,
            StepMode = VertexStepMode.Vertex,
            AttributeCount = attributeCount,
            Attributes = pAttributes
        };

        var blendState = new BlendState
        {
            Color = new BlendComponent { SrcFactor = BlendFactor.SrcAlpha, DstFactor = BlendFactor.OneMinusSrcAlpha, Operation = BlendOperation.Add },
            Alpha = new BlendComponent { SrcFactor = BlendFactor.One, DstFactor = BlendFactor.One, Operation = BlendOperation.Add }
        };

        var colorTarget = new ColorTargetState
        {
            Format = context.SwapChainFormat,
            WriteMask = ColorWriteMask.All,
            Blend = &blendState
        };

        var fragmentState = new FragmentState
        {
            Module = pShaderModule,
            EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("fs_main"),
            TargetCount = 1,
            Targets = &colorTarget
        };

        var pipelineDesc = new RenderPipelineDescriptor
        {
            Layout = pipelineLayout,
            Vertex = new VertexState
            {
                Module = pShaderModule,
                EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("vs_main"),
                BufferCount = 1,
                Buffers = &bufferLayout
            },
            Primitive = new PrimitiveState
            {
                Topology = PrimitiveTopology.TriangleList,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None
            },
            Multisample = new MultisampleState
            {
                Count = 1,
                Mask = 0xFFFFFFFF,
                AlphaToCoverageEnabled = false
            },
            Fragment = &fragmentState
        };

        var renderPipeline = webGpu.DeviceCreateRenderPipeline(context.Device, &pipelineDesc);

        Marshal.FreeHGlobal((IntPtr)fragmentState.EntryPoint);
        Marshal.FreeHGlobal((IntPtr)pipelineDesc.Vertex.EntryPoint);
        webGpu.PipelineLayoutRelease(pipelineLayout);
        webGpu.ShaderModuleRelease(pShaderModule);

        return new WebGPURenderPipeline(webGpu, renderPipeline, uniformGroupLayout, textureGroupLayout);
    }

    internal BindGroup* CreateUniformBindGroup(Device* device, Buffer* uniformBuffer, ulong bufferSize)
    {
        var entry = new BindGroupEntry
        {
            Binding = 0,
            Buffer = uniformBuffer,
            Offset = 0,
            Size = bufferSize
        };
        var desc = new BindGroupDescriptor
        {
            Layout = _uniformGroupLayout,
            EntryCount = 1,
            Entries = &entry
        };
        return _webGpu.DeviceCreateBindGroup(device, &desc);
    }

    internal BindGroup* CreateTextureBindGroup(Device* device, TextureView* textureView, Sampler* sampler)
    {
        var entries = stackalloc BindGroupEntry[2];
        entries[0] = new BindGroupEntry { Binding = 0, TextureView = textureView };
        entries[1] = new BindGroupEntry { Binding = 1, Sampler = sampler };
        var desc = new BindGroupDescriptor
        {
            Layout = _textureGroupLayout,
            EntryCount = 2,
            Entries = entries
        };
        return _webGpu.DeviceCreateBindGroup(device, &desc);
    }

    private WebGPURenderPipeline(Silk.NET.WebGPU.WebGPU webGpu, RenderPipeline* renderPipeline, BindGroupLayout* uniformGroupLayout, BindGroupLayout* textureGroupLayout)
    {
        _webGpu = webGpu;
        RenderPipeline = renderPipeline;
        _uniformGroupLayout = uniformGroupLayout;
        _textureGroupLayout = textureGroupLayout;
    }

    public void Dispose()
    {
        _webGpu.BindGroupLayoutRelease(_uniformGroupLayout);
        _webGpu.BindGroupLayoutRelease(_textureGroupLayout);
        _webGpu.RenderPipelineRelease(RenderPipeline);
    }
}
