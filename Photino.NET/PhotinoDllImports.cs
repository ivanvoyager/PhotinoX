using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using GetAllMonitorsCallback = Photino.NET.NativeDelegates.MonitorCallback;
using InvokeCallback = Photino.NET.NativeDelegates.VoidCallback;

namespace Photino.NET;

/// <summary>
/// The PhotinoWindow class represents a window in a Photino-based desktop application.
/// </summary>
public partial class PhotinoWindow
{
    private const string DLL_NAME = "PhotinoX.Native";

    //REGISTER
    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr Photino_register_win32(IntPtr hInstance);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr Photino_register_mac();



    //CTOR-DTOR
#pragma warning disable SYSLIB1054
    //Not useful to use LibraryImport when passing a user-defined type.
    //See https://stackoverflow.com/questions/77770231/libraryimport-the-type-is-not-supported-by-source-generated-p-invokes
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Ansi)]
    private static extern IntPtr Photino_ctor(ref PhotinoNativeParameters parameters);
#pragma warning restore SYSLIB1054

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_dtor(IntPtr instance);


    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_AddCustomSchemeName(IntPtr instance, string scheme);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_Close(IntPtr instance);


    //GET
    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr Photino_getHwnd_win32(IntPtr instance);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetAllMonitors(IntPtr instance, GetAllMonitorsCallback callback);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetTransparentEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetContextMenuEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetZoomEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetDevToolsEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetFullScreen(IntPtr instance, out byte fullScreen);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetGrantBrowserPermissions(IntPtr instance, out byte grant);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr Photino_GetUserAgent(IntPtr instance);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetMediaAutoplayEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetFileSystemAccessEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetWebSecurityEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetJavascriptClipboardAccessEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetMediaStreamEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetSmoothScrollingEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetIgnoreCertificateErrorsEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetNotificationsEnabled(IntPtr instance, out byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetPosition(IntPtr instance, out int x, out int y);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetResizable(IntPtr instance, out byte resizable);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial uint Photino_GetScreenDpi(IntPtr instance);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetSize(IntPtr instance, out int width, out int height);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial IntPtr Photino_GetTitle(IntPtr instance);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetTopmost(IntPtr instance, out byte topmost);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetZoom(IntPtr instance, out int zoom);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetMaximized(IntPtr instance, out byte maximized);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_GetMinimized(IntPtr instance, out byte minimized);


    //MARSHAL CALLS FROM Non-UI Thread to UI Thread
    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_Invoke(IntPtr instance, InvokeCallback callback);


    //NAVIGATE
    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_NavigateToString(IntPtr instance, string? content);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_NavigateToUrl(IntPtr instance, string url);


    //SET
    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_setWebView2RuntimePath_win32(IntPtr instance, string webView2RuntimePath);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetTransparentEnabled(IntPtr instance, byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetContextMenuEnabled(IntPtr instance, byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetZoomEnabled(IntPtr instance, byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetDevToolsEnabled(IntPtr instance, byte enabled);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetFullScreen(IntPtr instance, byte fullScreen);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetGrantBrowserPermissions(IntPtr instance, byte grant);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMaximized(IntPtr instance, byte maximized);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMaxSize(IntPtr instance, int maxWidth, int maxHeight);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMinimized(IntPtr instance, byte minimized);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetMinSize(IntPtr instance, int minWidth, int minHeight);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetResizable(IntPtr instance, byte resizable);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetPosition(IntPtr instance, int x, int y);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetSize(IntPtr instance, int width, int height);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetTitle(IntPtr instance, string? title);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetTopmost(IntPtr instance, byte topmost);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetIconFile(IntPtr instance, string? filename);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SetZoom(IntPtr instance, int zoom);


    //MISC
    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_Center(IntPtr instance);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_ClearBrowserAutoFill(IntPtr instance);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_SendWebMessage(IntPtr instance, string message);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_ShowMessage(IntPtr instance, string title, string body, uint type);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_ShowNotification(IntPtr instance, string title, string body);

    [LibraryImport(DLL_NAME, SetLastError = true)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial void Photino_WaitForExit(IntPtr instance);


    //DIALOG
    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr Photino_ShowOpenFile(IntPtr inst, string title, string defaultPath, byte multiSelect, [In] string[] filters, int filtersCount, out int resultCount);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr Photino_ShowOpenFolder(IntPtr inst, string title, string defaultPath, byte multiSelect, out int resultCount);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial IntPtr Photino_ShowSaveFile(IntPtr inst, string title, string defaultPath, [In] string[] filters, int filtersCount, string defaultFileName);

    [LibraryImport(DLL_NAME, SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial PhotinoDialogResult Photino_ShowMessage(IntPtr inst, string title, string text, PhotinoDialogButtons buttons, PhotinoDialogIcon icon);
}
