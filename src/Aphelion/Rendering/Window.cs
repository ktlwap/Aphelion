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
    private IInputContext? _inputContext;
    private WebGPUContext? _webGpuContext;
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
        options.VSync = windowCreationOptions.VSync;

        return new Window(Silk.NET.Windowing.Window.Create(options));
    }
    
    private Window(IWindow nativeWindow)
    {
        _nativeWindow = nativeWindow;
        _nativeWindow.Load += Load;
        _nativeWindow.Update += Update;
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
        
        _webGpuContext = WebGPUContext.Create(_nativeWindow);
    }

    private void Update(double obj)
    {
        Input.Refresh();
    }
    
    private void Render(double obj)
    {
        DrawCommandBuffer drawCommandBuffer = _webGpuContext!.CreateCommandBuffer();
        
        foreach (BaseComponent component in ComponentCache.Instance.Value!.Components)
            component.Render(drawCommandBuffer);
        
        _webGpuContext.QueueCommandBuffer(drawCommandBuffer);
    }

    private void Closing()
    {
        Dispose();
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