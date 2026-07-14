using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

internal static partial class NativeMethods
{
    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_NavigateToString(IntPtr instance, string? content);

    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_NavigateToUrl(IntPtr instance, string url);


    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_SendWebMessage(IntPtr instance, string message);


    [LibraryImport(DLL_NAME, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool Photino_AddCustomSchemeName(IntPtr instance, string scheme);


    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_ClearBrowserAutoFill(IntPtr instance);


    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetContextMenuEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_SetContextMenuEnabled(IntPtr instance, byte enabled);
    

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetZoomEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_SetZoomEnabled(IntPtr instance, byte enabled);


    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetZoom(IntPtr instance, out int zoom);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_SetZoom(IntPtr instance, int zoom);


    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetDevToolsEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_SetDevToolsEnabled(IntPtr instance, byte enabled);


    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetGrantBrowserPermissions(IntPtr instance, out byte grant);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial IntPtr Photino_GetUserAgent(IntPtr instance);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetMediaAutoplayEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetFileSystemAccessEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetWebSecurityEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetJavascriptClipboardAccessEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetMediaStreamEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetSmoothScrollingEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void Photino_GetIgnoreCertificateErrorsEnabled(IntPtr instance, out byte enabled);
}
