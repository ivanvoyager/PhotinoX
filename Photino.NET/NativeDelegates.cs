using System.Runtime.InteropServices;

namespace Photino.NET;

internal static class NativeDelegates
{
    private const CallingConvention CC = CallingConvention.Cdecl;
    //These are for the callbacks from C++ to C#.

    //These are wired up automatically in the PhotinoWindow (.NET) constructor.
    [UnmanagedFunctionPointer(CC)] internal delegate void VoidCallback();
    [UnmanagedFunctionPointer(CC)] internal delegate void VoidStateCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] internal delegate void VoidBoolCallback([MarshalAs(UnmanagedType.I1)] bool value);
    [UnmanagedFunctionPointer(CC)] internal delegate byte BoolCallback();    //C++ uses 1 byte for bool, C# uses 4 bytes
    [UnmanagedFunctionPointer(CC)] internal delegate void IntIntCallback(int a, int b);
    [UnmanagedFunctionPointer(CC)] internal delegate void StringCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string message);
    [UnmanagedFunctionPointer(CC)] internal delegate IntPtr ResourceCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string url, out int outNumBytes, out IntPtr outContentType);
    [UnmanagedFunctionPointer(CC)] internal delegate void StateChangedCallback([MarshalAs(UnmanagedType.I4)] PhotinoWindowState oldState, [MarshalAs(UnmanagedType.I4)] PhotinoWindowState newState);

    //These are sent in during the request
    [UnmanagedFunctionPointer(CC)] internal delegate int MonitorCallback(in NativeMonitor monitor, IntPtr state);
}