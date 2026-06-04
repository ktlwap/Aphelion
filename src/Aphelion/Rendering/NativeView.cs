using System.Runtime.InteropServices;
using Silk.NET.GLFW;

namespace Aphelion.Rendering;

public abstract unsafe class NativeView
{
    protected readonly Glfw _glfw;
    protected readonly WindowHandle* _pWindowHandle;
    
    public (nint Display, nuint Window)? X11 { get; private set; }
    
    public nint? Cocoa { get; private set; }
    
    public (nint Display, nint Surface)? Wayland { get; private set; }
    
    public nint? WinRT { get; private set; }
    
    public (nint Window, uint Framebuffer, uint Colorbuffer, uint ResolveFramebuffer)? UIKit { get; private set; }
    
    public (nint Hwnd, nint HDC, nint HInstance)? Win32 { get; private set; }
    
    public (nint Display, nint Window)? Vivante { get; private set; }
    
    public (nint Window, nint Surface)? Android { get; private set; }
    
    public nint? DXHandle { get; private set; }
    
    [DllImport("user32", EntryPoint = "GetDC")]
    private static extern nint Win32GetDC(nint hwnd);
    
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);
    
    protected NativeView(Glfw glfw, WindowHandle* pWindowHandle)
    {
        _glfw = glfw;
        _pWindowHandle = pWindowHandle;
        
        if (_glfw.Context.TryGetProcAddress("glfwGetWin32Window", out var getHwnd))
        {
            var hwnd = ((delegate* unmanaged[Cdecl]<WindowHandle*, nint>) getHwnd)(_pWindowHandle);
            Win32 = (hwnd, Win32GetDC(hwnd), GetWindowLongPtr64(hwnd, -6));
            DXHandle = hwnd;
        }
        else if (_glfw.Context.TryGetProcAddress("glfwGetCocoaWindow", out var getCocoaId))
        {
            Cocoa = (nint) ((delegate* unmanaged[Cdecl]<WindowHandle*, void*>)getCocoaId)(_pWindowHandle);
        }
        else if (_glfw.Context.TryGetProcAddress("glfwGetX11Display", out var getX11Display) && 
                 _glfw.Context.TryGetProcAddress("glfwGetX11Window", out var getX11Window))
        {
            var x11Display = (nint) ((delegate* unmanaged[Cdecl]<void*>) getX11Display)();
            var x11Window = ((delegate* unmanaged[Cdecl]<WindowHandle*, nuint>) getX11Window)(_pWindowHandle);
                
            if (x11Display != 0 && x11Window != 0)
                X11 = (x11Display, x11Window);
        }
        else if (_glfw.Context.TryGetProcAddress("glfwGetWaylandDisplay", out var getWaylandDisplay) && 
                 _glfw.Context.TryGetProcAddress("glfwGetWaylandWindow", out var getWaylandWindow))
        {
            Wayland = ((nint) ((delegate* unmanaged[Cdecl]<void*>) getWaylandDisplay)(),
                (nint) ((delegate* unmanaged[Cdecl]<WindowHandle*, void*>) getWaylandWindow)(_pWindowHandle)); 
        }
    }
}