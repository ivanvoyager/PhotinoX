using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

internal static partial class NativeMethods
{
    private const string DLL_NAME = "PhotinoX.Native";

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_register_win32(IntPtr hInstance);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_register_mac();

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_register_linux();

#pragma warning disable SYSLIB1054
    //Not useful to use LibraryImport when passing a user-defined type.
    //See https://stackoverflow.com/questions/77770231/libraryimport-the-type-is-not-supported-by-source-generated-p-invokes
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr Photino_ctor(ref PhotinoWindowNativeParameters parameters);
#pragma warning restore SYSLIB1054

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_GetNativeVersion();

#pragma warning disable SYSLIB1054
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    internal static extern PhotinoNativeRuntimeInfo Photino_GetRuntimeInfo();
#pragma warning restore SYSLIB1054

    internal static string? GetNativeVersion()
    {
        var ptr = Photino_GetNativeVersion();
        return ptr != IntPtr.Zero ? Marshal.PtrToStringUTF8(ptr) : null;
    }

    internal static string? PtrToStringUTF8(IntPtr value)
    {
        return Marshal.PtrToStringUTF8(value);
    }
}