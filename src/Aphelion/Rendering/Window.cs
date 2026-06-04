using System.Numerics;
using Aphelion.Caches;
using Aphelion.Core;
using Aphelion.Rendering.WebGPU;
using Silk.NET.GLFW;

namespace Aphelion.Rendering;

public struct WindowCreationOptions
{
    public required string Title;
    public required int Width;
    public required int Height;
    public bool VSync;
}

public unsafe class Window : NativeView, IDisposable
{
    private Input? _input;
    private WebGPUContext? _webGpuContext;

    public Vector2 Size { get; private set; }
    
    public BaseScene? CurrentScene { get; private set; }

    public static Window Create(WindowCreationOptions windowCreationOptions)
    {
        var glfw = Glfw.GetApi();
        if (!glfw.Init())
            throw new InvalidOperationException("Failed to initialize GLFW.");

        glfw.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
        glfw.WindowHint(WindowHintBool.Visible, true);
        glfw.WindowHint(WindowHintBool.Resizable, false);

        var pWindowHandle = glfw.CreateWindow(
            windowCreationOptions.Width,
            windowCreationOptions.Height,
            windowCreationOptions.Title,
            null,
            null);
        
        glfw.MakeContextCurrent(pWindowHandle);
        
        if (windowCreationOptions.VSync)
            glfw.SwapInterval(1);

        if (pWindowHandle == null)
        {
            glfw.Terminate();
            throw new InvalidOperationException("Failed to create GLFW window.");
        }

        return new Window(glfw, pWindowHandle);
    }


    private Window(Glfw glfw, WindowHandle* pWindowHandle) : base(glfw, pWindowHandle) { }

    public void Run<TScene>()
        where TScene : BaseScene, new()
    {
        Load<TScene>();
        
        while (!_glfw.WindowShouldClose(_pWindowHandle))
        {
            Update();
            Render();
        }
    }

    private void Load<TScene>()
        where TScene : BaseScene, new()
    {
        UpdateWindowInternalStates();
        
        _input = new Input(_glfw, _pWindowHandle);
        _webGpuContext = WebGPUContext.Create(_glfw, this);
        
        var shaderSource = File.ReadAllText("Assets/Shaders/shader.wgsl");
        _webGpuContext.Setup(shaderSource);
        
        CurrentScene = new TScene();
        CurrentScene.Start();
    }

    private void Update()
    {
        UpdateWindowInternalStates();
        
        _input.Refresh();
        ComponentCache.Instance.Value.Update();
        ComponentCache.Instance.Value.Update();
        foreach (BaseComponent component in ComponentCache.Instance.Value!.Components)
        {
            component.Update();
        }
    }

    private void Render()
    {
        _glfw.SwapBuffers( _pWindowHandle);
        
        DrawCommandBuffer worldBuffer = _webGpuContext.CreateCommandBuffer();
        DrawCommandBuffer uiBuffer = _webGpuContext.CreateCommandBuffer();

        foreach (BaseComponent component in ComponentCache.Instance.Value.Components)
        {
            component.Render(worldBuffer);
            component.RenderUI(uiBuffer);
        }

        _webGpuContext.QueueCommandBuffer(worldBuffer, uiBuffer);
    }

    public void Close()
    {
        _glfw.DestroyWindow( _pWindowHandle);
    }

    private void UpdateWindowInternalStates()
    {
        _glfw.GetWindowSize(_pWindowHandle, out var width, out var height);
        Size = new Vector2(width, height);

        var previousTotalTime = Time.Total;
        var newTotalTime = _glfw.GetTime();
        var deltaTime = newTotalTime - previousTotalTime;

        Time.Total = newTotalTime;
        Time.Delta = deltaTime;
        Time.TotalF = (float)newTotalTime;
        Time.DeltaF = (float)deltaTime;
    }

    public void Dispose()
    {
        CurrentScene?.Stop();
        
        _glfw.Terminate();
        _glfw.Dispose();
    }
}
