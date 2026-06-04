using Aphelion.Caches;
using Aphelion.Core;
using Aphelion.Rendering.WebGPU;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Aphelion.Rendering;

public struct WindowCreationOptions
{
    public required string Title;
    public required int Width;
    public required int Height;
    public bool VSync;
}

/// <summary>
/// Represents a graphical application window that can be created, managed, and rendered.
/// </summary>
/// <remarks>
/// The <see cref="Window"/> class allows developers to create and manage application windows with
/// customizable parameters such as size, title, and graphics settings. This class also provides
/// functionality for managing a main application window and supports multiple window instances.
/// </remarks>
public unsafe class Window : IDisposable
{
    private static Window? _mainWindow;
    private static List<Window> _windows = new();

    private readonly IWindow _nativeWindow;
    private readonly bool _vsync;
    private Input? _input;
    private WebGPUContext? _webGpuContext;
    private volatile bool _stopUpdateThread;
    private bool _isDisposed;

    public Input Input => _input ?? throw new InvalidOperationException("Input is not initialized until the window has loaded.");

    /// <summary>
    /// Gets the reference to the main application window.
    /// </summary>
    /// <remarks>
    /// The main window is the first window created and run within the application.
    /// If no windows have been created or the main window has not been set,
    /// this property will return <c>null</c>.
    /// </remarks>
    /// <returns>
    /// The main <see cref="Window"/> instance or <c>null</c> if no main window exists.
    /// </returns>
    public static Window? MainWindow => _mainWindow;

    /// <summary>
    /// Gets a read-only collection of all created windows within the application.
    /// </summary>
    /// <remarks>
    /// This property provides access to a list of all window instances currently managed by the application.
    /// Each window in the collection is created using the <see cref="Create"/> method and added to the list
    /// automatically when the <see cref="Run"/> method is called for the window.
    /// </remarks>
    /// <returns>
    /// A read-only collection of <see cref="Window"/> instances representing all created windows.
    /// </returns>
    public static IReadOnlyList<Window> Windows => _windows.AsReadOnly();
    
    public BaseScene CurrentScene { get; private set; }

    /// <summary>
    /// Creates a new instance of the <see cref="Window"/> class with the specified creation options.
    /// </summary>
    /// <param name="windowCreationOptions">
    /// An instance of <see cref="WindowCreationOptions"/> specifying the parameters
    /// for the window such as title, width, and height.
    /// </param>
    /// <returns>
    /// A new <see cref="Window"/> instance initialized with the provided creation options.
    /// </returns>
    public static Window Create(WindowCreationOptions windowCreationOptions)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(windowCreationOptions.Width, windowCreationOptions.Height);
        options.Title = windowCreationOptions.Title;
        options.API = GraphicsAPI.None;
        options.IsVisible = true;
        options.ShouldSwapAutomatically = false;
        options.IsContextControlDisabled = true;

        return new Window(Silk.NET.Windowing.Window.Create(options), windowCreationOptions.VSync);
    }

    private Window(IWindow nativeWindow, bool vsync)
    {
        _nativeWindow = nativeWindow;
        _vsync = vsync;
        _nativeWindow.Load += Load;
        _nativeWindow.Render += Render;
        _nativeWindow.Update += Update;
        _nativeWindow.Closing += Closing;
    }

    /// <summary>
    /// Runs the window's main event loop, rendering its contents and handling user input.
    /// </summary>
    /// <remarks>
    /// This method initiates the execution of the window. If this is the first window being run,
    /// it will become the application's main window. Subsequent windows will be executed on separate threads.
    /// </remarks>
    public void Run<TScene>() where TScene : BaseScene, new()
    {
        CurrentScene = new TScene();
        _windows.Add(this);
        
        if (MainWindow == null)
        {
            _mainWindow = this;
            _nativeWindow.Run();
        }
        else
            throw new Exception("Cannot run multiple windows.");
    }

    /// <summary>
    /// Closes the window and releases all associated resources.
    /// </summary>
    public void Close()
    {
        _nativeWindow.Close();
        _nativeWindow.Dispose();
    }

    private void Load()
    {
        var glfwHandle = _nativeWindow.Native?.Glfw
            ?? throw new InvalidOperationException("Window backend is not GLFW; cannot initialize Input.");
        _input = new Input(Glfw.GetApi(), (WindowHandle*)glfwHandle);

        _webGpuContext = WebGPUContext.Create(_nativeWindow, _vsync);

        var shaderSource = File.ReadAllText("Assets/Shaders/shader.wgsl");
        _webGpuContext.Setup(shaderSource);
        
        CurrentScene.Start();
    }

    private void Update(double delta)
    {
        _input?.Refresh();
        ComponentCache.Instance.Value.Update();
        ComponentCache.Instance.Value.Update();
        foreach (BaseComponent component in ComponentCache.Instance.Value!.Components)
        {
            component.Update();
        }
    }

    private void Render(double obj)
    {
        DrawCommandBuffer worldBuffer = _webGpuContext!.CreateCommandBuffer();
        DrawCommandBuffer uiBuffer = _webGpuContext!.CreateCommandBuffer();

        foreach (BaseComponent component in ComponentCache.Instance.Value!.Components)
        {
            component.Render(worldBuffer);
            component.RenderUI(uiBuffer);
        }

        _webGpuContext.QueueCommandBuffer(worldBuffer, uiBuffer);
    }

    private void Closing()
    {
        CurrentScene.Stop();
        _stopUpdateThread = true;
    }
    
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _webGpuContext?.Dispose();
            _nativeWindow.Dispose();
        }
    }
}
