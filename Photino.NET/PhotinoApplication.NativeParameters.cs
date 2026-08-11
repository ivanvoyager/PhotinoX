using System.Runtime.InteropServices;

using StartupCallback = Photino.NET.NativeDelegates.VoidCallback;
using ExitCallback = Photino.NET.NativeDelegates.ExitCallback;
using NotificationActivatedCallback = Photino.NET.NativeDelegates.NotificationActivatedCallback;
using NotificationActionActivatedCallback = Photino.NET.NativeDelegates.NotificationActionActivatedCallback;
using NotificationInputActivatedCallback = Photino.NET.NativeDelegates.NotificationInputActivatedCallback;
using NotificationDismissedCallback = Photino.NET.NativeDelegates.NotificationDismissedCallback;
using NotificationFailedCallback = Photino.NET.NativeDelegates.NotificationFailedCallback;

namespace Photino.NET;

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoApplicationNativeParameters
{
    internal const int NativeAbiVersion = 1;

    [MarshalAs(UnmanagedType.I4)] internal int Size; //#1
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion; // #2

    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? ApplicationName;//#3
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? ApplicationIconPath; //#4
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? NotificationRegistrationId;//#5

    [MarshalAs(UnmanagedType.FunctionPtr)] internal StartupCallback? StartupHandler;//6
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ExitCallback? ExitHandler;//7

    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationActivatedCallback? NotificationActivatedHandler; //#8
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationActionActivatedCallback? NotificationActionActivatedHandler; //#9
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationInputActivatedCallback? NotificationInputActivatedHandler; //#10
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationDismissedCallback? NotificationDismissedHandler; //#11
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationFailedCallback? NotificationFailedHandler; //#12

    [MarshalAs(UnmanagedType.I1)] internal bool NotificationsEnabled; //#13
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNotificationShowNativeParameters
{
    internal const int NativeAbiVersion = 1;

    [MarshalAs(UnmanagedType.I4)] internal int Size;
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion;

    [MarshalAs(UnmanagedType.I4)] internal int NotificationId;

    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? Title;
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? Body;
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? IconPath;

    internal IntPtr CallbackState;
}