using Silk.NET.Core.Native;

namespace Aphelion.Rendering.WebGPU.MacOS;

internal struct Selector
{
    public readonly nint NativePtr;

    public Selector(nint ptr)
    {
        NativePtr = ptr;
    }

    public Selector(string name)
    {
        var namePtr = SilkMarshal.StringToPtr(name);
        NativePtr = ObjectiveCRuntime.sel_registerName(namePtr);
        SilkMarshal.Free(namePtr);
    }

    public static implicit operator Selector(string s)
    {
        return new Selector(s);
    }
}
