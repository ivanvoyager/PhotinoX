using System.Runtime.InteropServices;

namespace Photino.NET;

internal static class NativeDelegates
{
    private const CallingConvention CC = CallingConvention.Cdecl;
    //These are for the callbacks from C++ to C#.

    //These are wired up automatically in the PhotinoWindow (.NET) constructor.
    [UnmanagedFunctionPointer(CC)] public delegate void VoidCallback();
    [UnmanagedFunctionPointer(CC)] public delegate void VoidStateCallback(IntPtr state);
    [UnmanagedFunctionPointer(CC)] public delegate void VoidBoolCallback([MarshalAs(UnmanagedType.I1)] bool value);
    [UnmanagedFunctionPointer(CC)] public delegate byte BoolCallback();    //C++ uses 1 byte for bool, C# uses 4 bytes
    [UnmanagedFunctionPointer(CC)] public delegate void IntIntCallback(int a, int b);
    [UnmanagedFunctionPointer(CC)] public delegate void StringCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string message);
    [UnmanagedFunctionPointer(CC)] public delegate IntPtr ResourceCallback([MarshalAs(UnmanagedType.LPUTF8Str)] string url, out int outNumBytes, out IntPtr outContentType);

    //These are sent in during the request
    [UnmanagedFunctionPointer(CC)] public delegate int MonitorCallback(in NativeMonitor monitor);
}