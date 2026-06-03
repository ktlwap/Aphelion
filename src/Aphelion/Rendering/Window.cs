using Aphelion.Caches;
using Aphelion.Core;
using Aphelion.Rendering.WebGPU;
using Silk.NET.Input;
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
public class Window : IDisposable
{
    private static Window? _mainWindow;
    private static List<Window> _windows = new();
    
    private readonly IWindow _nativeWindow;
    private readonly bool _vsync;
    private IInputContext? _inputContext;
    private WebGPUContext? _webGpuContext;
    private Thread? _updateThread;
    private volatile bool _stopUpdateThread;
    private bool _isDisposed;

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
        _nativeWindow.Closing += Closing;
    }

    /// <summary>
    /// Runs the window's main event loop, rendering its contents and handling user input.
    /// </summary>
    /// <remarks>
    /// This method initiates the execution of the window. If this is the first window being run,
    /// it will become the application's main window. Subsequent windows will be executed on separate threads.
    /// </remarks>
    public void Run()
    {
        _windows.Add(this);
        
        if (MainWindow == null)
        {
            _mainWindow = this;
            _nativeWindow.Run();
        }
        else
            new Thread(Run).Start();
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
        _inputContext = _nativeWindow.CreateInput();
        Input.CreateInstance(_inputContext);

        _webGpuContext = WebGPUContext.Create(_nativeWindow, _vsync);

        var shaderSource = File.ReadAllText("Assets/Shaders/shader.wgsl");
        _webGpuContext.Setup(shaderSource);

        // Adopt the render thread's per-window state on the update thread so both
        // see the same component cache, game-object cache, input and camera.
        var sharedComponents = ComponentCache.Instance.Value!;
        var sharedGameObjects = GameObjectCache.Instance.Value!;
        var sharedInput = Input.Instance.Value!;

        _stopUpdateThread = false;
        _updateThread = new Thread(() => UpdateLoop(sharedComponents, sharedGameObjects, sharedInput))
        {
            IsBackground = true,
            Name = "Aphelion-Update"
        };
        _updateThread.Start();
    }

    private void UpdateLoop(ComponentCache components, GameObjectCache gameObjects, Input input)
    {
        ComponentCache.Instance.Value = components;
        GameObjectCache.Instance.Value = gameObjects;
        Input.Instance.Value = input;

        while (!_stopUpdateThread)
        {
            gameObjects.Update();
            components.Update();
            Input.Refresh();
            foreach (BaseComponent component in components.Components)
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
        _stopUpdateThread = true;
        _updateThread?.Join();
    }
    
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _inputContext?.Dispose();
            _webGpuContext?.Dispose();
            _nativeWindow.Dispose();
        } 
    }
}