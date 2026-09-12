using System.Runtime.InteropServices;

namespace Photino.NET;

internal static class NativeDelegates
{
    private const CallingConvention CC = CallingConvention.Cdecl;

    [UnmanagedFunctionPointer(CC)] internal delegate void VoidStateCallback(IntPtr state);

    //Window callbacks
    [UnmanagedFunctionPointer(CC)] internal delegate void CreatedCallback(IntPtr instance, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate byte ClosingCallback(IntPtr state); //C++ uses 1 byte for bool, C# uses 4 bytes
    [UnmanagedFunctionPointer(CC)] internal delegate void ClosedCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void FocusInCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void FocusOutCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void ResizedCallback(int width, int height, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void MovedCallback(int x, int y, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void MaximizedCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void RestoredCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void MinimizedCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void FullScreenChangedCallback([MarshalAs(UnmanagedType.I1)] bool value, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void StateChangedCallback([MarshalAs(UnmanagedType.I4)] PhotinoWindowState oldState, [MarshalAs(UnmanagedType.I4)] PhotinoWindowState newState, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void WebMessageReceivedCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string message, [MarshalAs(UnmanagedType.LPUTF8Str)] string uri, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate IntPtr CustomSchemeCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string url, out int outNumBytes, out IntPtr outContentType, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate byte NavigationStartingCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string uri, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate byte NewWindowRequestedCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string uri, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void ContentLoadingCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string uri, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void ContentLoadedCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string uri, IntPtr state);

    [UnmanagedFunctionPointer(CC)] internal delegate byte GetAllMonitorsCallback(in NativeMonitor monitor, IntPtr state);

    //Application callbacks
    [UnmanagedFunctionPointer(CC)] internal delegate void StartupCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate byte ShutdownRequestedCallback([MarshalAs(UnmanagedType.I4)] PhotinoShutdownRequestReason reason, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate int ExitCallback(int exitCode, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void NotificationActivatedCallback(int notificationId, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void NotificationActionActivatedCallback(int notificationId, int actionIndex, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void NotificationInputActivatedCallback(int notificationId, [MarshalAs(UnmanagedType.LPUTF8Str)] string response, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void NotificationDismissedCallback(int notificationId, [MarshalAs(UnmanagedType.I4)] NotificationDismissalReason reason, IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void NotificationFailedCallback(int notificationId, IntPtr state);
}