using System.Runtime.InteropServices;
using Silk.NET.WebGPU;

namespace Aphelion.Rendering.WebGPU;

internal unsafe class WebGPURenderPipeline : IDisposable
{
    private readonly Silk.NET.WebGPU.WebGPU _webGpu;
    private readonly RenderPipeline* pRenderPipeline;

    public static WebGPURenderPipeline Compile(Silk.NET.WebGPU.WebGPU webGpu, WebGPUContext context, WebGPUBuffer<uint> uniformBuffer, string shaderSource, VertexAttribute* pAttributes, uint attributeCount, uint stride)
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
        
        // TODO:
        BindGroupLayoutEntry uniformLayoutEntry = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
            Buffer = new BufferBindingLayout
            {
                Type = BufferBindingType.Uniform,
                HasDynamicOffset = true,
                MinBindingSize = 64
            }
        };

        BindGroupLayoutDescriptor quadLayoutDesc = new BindGroupLayoutDescriptor
        {
            EntryCount = 1,
            Entries = &uniformLayoutEntry
        };
        BindGroupLayout* uniformGroupLayout = webGpu.DeviceCreateBindGroupLayout(context.Device, &quadLayoutDesc);
        
        BindGroupEntry uniformBindGroupEntry = new BindGroupEntry
        {
            Binding = 0,
            Buffer = uniformBuffer.Buffer,
            Size = uniformBuffer.Size,
        };

        BindGroupDescriptor quadBindGroupDesc = new BindGroupDescriptor
        {
            Layout = uniformGroupLayout,
            EntryCount = 1,
            Entries = &uniformBindGroupEntry
        };
        BindGroup* bindGroup = webGpu.DeviceCreateBindGroup(context.Device, &quadBindGroupDesc);
        
        BindGroupLayoutEntry textureEntry = new BindGroupLayoutEntry
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
        
        BindGroupLayoutEntry samplerEntry = new BindGroupLayoutEntry
        {
            Binding = 1,
            Visibility = ShaderStage.Fragment,
            Sampler = new SamplerBindingLayout
            {
                Type = SamplerBindingType.Filtering
            }
        };
        
        BindGroupLayoutEntry* spriteLayoutEntries = stackalloc BindGroupLayoutEntry[2];
        spriteLayoutEntries[0] = textureEntry;
        spriteLayoutEntries[1] = samplerEntry;

        BindGroupLayoutDescriptor spriteLayoutDesc = new BindGroupLayoutDescriptor
        {
            EntryCount = 2,
            Entries = spriteLayoutEntries
        };
        BindGroupLayout* textureGroupLayout = webGpu.DeviceCreateBindGroupLayout(context.Device, &spriteLayoutDesc);

        BindGroupLayout** bindGroupLayouts = stackalloc BindGroupLayout*[2];
        bindGroupLayouts[0] = uniformGroupLayout;
        bindGroupLayouts[1] = textureGroupLayout;
        
        VertexAttribute* vertexAttributes = stackalloc VertexAttribute[7];
        // Position -> vec2<f32>
        vertexAttributes[0] = new VertexAttribute { Format = VertexFormat.Float32x2, Offset = 0, ShaderLocation = 0 };
        // Uv -> vec2<f32>
        vertexAttributes[1] = new VertexAttribute { Format = VertexFormat.Float32x2, Offset = 2 * sizeof(float), ShaderLocation = 1 };
        // InstancePosition -> vec2<f32>
        vertexAttributes[2] = new VertexAttribute { Format = VertexFormat.Float32x2, Offset = 4 * sizeof(float), ShaderLocation = 2 };
        // InstanceScale -> vec2<f32>
        vertexAttributes[3] = new VertexAttribute { Format = VertexFormat.Float32x2, Offset = 6 * sizeof(float), ShaderLocation = 3 };
        // InstanceRotation -> f32
        vertexAttributes[4] = new VertexAttribute { Format = VertexFormat.Float32, Offset = 8 * sizeof(float), ShaderLocation = 4 };
        // InstanceZIndex -> f32
        vertexAttributes[5] = new VertexAttribute { Format = VertexFormat.Float32, Offset = 9 * sizeof(float), ShaderLocation = 5 };
        // InstanceColor -> vec4<f32>
        vertexAttributes[6] = new VertexAttribute { Format = VertexFormat.Float32x4, Offset = 10 * sizeof(float), ShaderLocation = 6 };

        PipelineLayoutDescriptor pipelineLayoutDesc = new PipelineLayoutDescriptor
        {
            BindGroupLayoutCount = 2,
            BindGroupLayouts = bindGroupLayouts
        };
        
        PipelineLayout* pipelineLayout = webGpu.DeviceCreatePipelineLayout(context.Device, &pipelineLayoutDesc);

        VertexBufferLayout bufferLayout = new VertexBufferLayout
        {
            ArrayStride = stride,
            StepMode = VertexStepMode.Vertex,
            AttributeCount = attributeCount,
            Attributes = pAttributes
        };

        BlendState blendState = new BlendState
        {
            Color = new BlendComponent { SrcFactor = BlendFactor.SrcAlpha, DstFactor = BlendFactor.OneMinusSrcAlpha, Operation = BlendOperation.Add },
            Alpha = new BlendComponent { SrcFactor = BlendFactor.One, DstFactor = BlendFactor.One, Operation = BlendOperation.Add }
        };

        ColorTargetState colorTarget = new ColorTargetState
        {
            Format = context.SwapChainFormat,
            WriteMask = ColorWriteMask.All,
            Blend = &blendState
        };

        FragmentState fragmentState = new FragmentState
        {
            Module = pShaderModule,
            EntryPoint = (byte*)Marshal.StringToHGlobalAnsi("fs_main"),
            TargetCount = 1,
            Targets = &colorTarget
        };

        RenderPipelineDescriptor pipelineDesc = new RenderPipelineDescriptor
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

        RenderPipeline* renderPipeline = webGpu.DeviceCreateRenderPipeline(context.Device, &pipelineDesc);

        Marshal.FreeHGlobal((IntPtr)fragmentState.EntryPoint);
        Marshal.FreeHGlobal((IntPtr)pipelineDesc.Vertex.EntryPoint);
        webGpu.PipelineLayoutRelease(pipelineLayout);
        webGpu.ShaderModuleRelease(pShaderModule);

        return new WebGPURenderPipeline(webGpu, renderPipeline, bindGroupLayouts);
    }

    private WebGPURenderPipeline(WebGPU webGpu, RenderPipeline* renderPipeline, BindGroup* bindGroup, BindGroupLayout* uniformGroupLayout, BindGroupLayout* textureGroupLayout)
    {
        _webGpu = webGpu;
        RenderPipeline = renderPipeline;
    }

    public void Dispose()
    {
        _webGpu.RenderPipelineRelease(RenderPipeline);
    }
    
    public void Dispose()
    {
        
    }
}