using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

internal static partial class NativeMethods
{
#pragma warning disable SYSLIB1054
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int PhotinoApplication_ShowNotification(ref PhotinoNotificationShowNativeParameters showParams);
#pragma warning restore SYSLIB1054

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void PhotinoApplication_GetNotificationsEnabled(out byte enabled);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void PhotinoApplication_SetNotificationsEnabled(byte enabled);
}