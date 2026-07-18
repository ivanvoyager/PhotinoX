using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

using InvokeCallback = NativeDelegates.VoidCallback;

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
    internal static extern IntPtr Photino_ctor(ref PhotinoNativeParameters parameters);
#pragma warning restore SYSLIB1054

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetNotificationsEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_ShowNotification(IntPtr instance, string title, string body);
}
