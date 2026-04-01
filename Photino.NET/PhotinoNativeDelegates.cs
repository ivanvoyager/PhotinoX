using System.Runtime.InteropServices;

namespace Photino.NET;

internal static class NativeDelegates
{
    private const CallingConvention CC = CallingConvention.Cdecl;
    private const CharSet CS = CharSet.Auto;

    //These are for the callbacks from C++ to C#.

    //These are wired up automatically in the PhotinoWindow (.NET) constructor.
    [UnmanagedFunctionPointer(CC, CharSet = CS)] public delegate void VoidCallback();
    [UnmanagedFunctionPointer(CC, CharSet = CS)] public delegate byte BoolCallback();    //C++ uses 1 byte for bool, C# uses 4 bytes
    [UnmanagedFunctionPointer(CC, CharSet = CS)] public delegate void IntIntCallback(int a, int b);
    [UnmanagedFunctionPointer(CC, CharSet = CS)] public delegate void StringCallback(string message);
    [UnmanagedFunctionPointer(CC, CharSet = CS)] public delegate IntPtr ResourceCallback(string url, out int outNumBytes, out string? outContentType);

    //These are sent in during the request
    [UnmanagedFunctionPointer(CC)] public delegate int MonitorCallback(in NativeMonitor monitor);
}