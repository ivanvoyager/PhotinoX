using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ClosingCallback = Photino.NET.NativeDelegates.BoolCallback;
using ClosedCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusInCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusOutCallback = Photino.NET.NativeDelegates.VoidCallback;
using ResizedCallback = Photino.NET.NativeDelegates.IntIntCallback; //(int width, int height)
using MovedCallback = Photino.NET.NativeDelegates.IntIntCallback;   //(int x, int y)
using MaximizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using RestoredCallback = Photino.NET.NativeDelegates.VoidCallback;
using MinimizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using FullScreenChangedCallback = Photino.NET.NativeDelegates.VoidBoolCallback;
using StateChangedCallback = Photino.NET.NativeDelegates.StateChangedCallback;

namespace Photino.NET;

internal static partial class NativeMethods
{
    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetClosingCallback(IntPtr instance, ClosingCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetClosedCallback(IntPtr instance, ClosedCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetFocusInCallback(IntPtr instance, FocusInCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetFocusOutCallback(IntPtr instance, FocusOutCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMovedCallback(IntPtr instance, MovedCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetResizedCallback(IntPtr instance, ResizedCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMaximizedCallback(IntPtr instance, MaximizedCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetRestoredCallback(IntPtr instance, RestoredCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMinimizedCallback(IntPtr instance, MinimizedCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetFullScreenChangedCallback(IntPtr instance, FullScreenChangedCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetStateChangedCallback(IntPtr instance, StateChangedCallback callback);

}
