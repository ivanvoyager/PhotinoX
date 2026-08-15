using System.Runtime.InteropServices;

using StartupCallback = Photino.NET.NativeDelegates.VoidCallback;
using ShutdownRequestedCallback = Photino.NET.NativeDelegates.ShutdownRequestedCallback;
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
    internal const int NativeAbiVersion = 2;

    [MarshalAs(UnmanagedType.I4)] internal int Size; //#1
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion; // #2

    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? ApplicationName;//#3
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? ApplicationIconPath; //#4
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? NotificationRegistrationId;//#5

    [MarshalAs(UnmanagedType.FunctionPtr)] internal StartupCallback? StartupHandler;//6
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ShutdownRequestedCallback? ShutdownRequestedHandler;//7
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ExitCallback? ExitHandler;//8

    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationActivatedCallback? NotificationActivatedHandler; //#9
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationActionActivatedCallback? NotificationActionActivatedHandler; //#10
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationInputActivatedCallback? NotificationInputActivatedHandler; //#11
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationDismissedCallback? NotificationDismissedHandler; //#12
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationFailedCallback? NotificationFailedHandler; //#13

    [MarshalAs(UnmanagedType.I1)] internal bool NotificationsEnabled; //#14
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNotificationShowNativeParameters
{
    internal const int NativeAbiVersion = 1;

    [MarshalAs(UnmanagedType.I4)] internal int Size; //#1
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion; //#2

    [MarshalAs(UnmanagedType.I4)] internal int NotificationId; //#3

    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? Title; //#4
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? Body; //#5
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? IconPath; //#6

    internal IntPtr CallbackState; //#7
}