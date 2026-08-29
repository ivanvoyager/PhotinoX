using System.Runtime.InteropServices;

using static Photino.NET.NativeDelegates;

namespace Photino.NET;

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeApplicationCallbackParameters
{
    [MarshalAs(UnmanagedType.FunctionPtr)] internal StartupCallback? StartupHandler; //1
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ShutdownRequestedCallback? ShutdownRequestedHandler; //2
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ExitCallback? ExitHandler; //3
    internal IntPtr CallbackState; //#4
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeApplicationOptions
{
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? ApplicationName;//#1
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? ApplicationIconPath; //#2
    [MarshalAs(UnmanagedType.LPUTF8Str)] internal string? NotificationRegistrationId;//#3

    [MarshalAs(UnmanagedType.I1)] internal bool NotificationsEnabled; //#4
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeNotificationCallbackParameters
{
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationActivatedCallback? NotificationActivatedHandler; //#1
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationActionActivatedCallback? NotificationActionActivatedHandler; //#2
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationInputActivatedCallback? NotificationInputActivatedHandler; //#3
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationDismissedCallback? NotificationDismissedHandler; //#4
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NotificationFailedCallback? NotificationFailedHandler; //#5
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoApplicationNativeParameters
{
    static PhotinoApplicationNativeParameters()
    {
        if (Marshal.OffsetOf<PhotinoApplicationNativeParameters>(nameof(Callbacks)).ToInt32() != 8 ||
            Marshal.OffsetOf<PhotinoApplicationNativeParameters>(nameof(Options)).ToInt32() != 40 ||
            Marshal.OffsetOf<PhotinoApplicationNativeParameters>(nameof(NotificationCallbacks)).ToInt32() != 72 ||
            Marshal.SizeOf<PhotinoApplicationNativeParameters>() != 112)
        {
            throw new TypeLoadException($"{typeof(PhotinoApplicationNativeParameters).FullName} has an invalid native layout.");
        }
    }

    internal const int NativeAbiVersion = 3;

    [MarshalAs(UnmanagedType.I4)] internal int Size; //#1
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion; // #2

    internal PhotinoNativeApplicationCallbackParameters Callbacks; //#3
    internal PhotinoNativeApplicationOptions Options; //#4
    internal PhotinoNativeNotificationCallbackParameters NotificationCallbacks; //#5
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