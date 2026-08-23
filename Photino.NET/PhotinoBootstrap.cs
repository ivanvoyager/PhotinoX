using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Photino.NET;

using static NativeMethods;

internal static class PhotinoBootstrap
{
    private static int s_initialized;

    static PhotinoBootstrap()
    {
        Debug.Assert(Marshal.SizeOf<NativeRect>() == 16);
        Debug.Assert(Marshal.SizeOf<NativeMonitor>() == 40);
        Debug.Assert(Marshal.SizeOf<NativeThickness>() == 16);
        Debug.Assert(Marshal.SizeOf<NativeLayoutRegion>() == 32);
    }

    internal static void Initialize()
    {
        if (Interlocked.CompareExchange(ref s_initialized, 1, 0) != 0)
            return;

        try
        {
            if (Platform.IsWindows)
            {
                var nativeType = NativeLibrary.GetMainProgramHandle();
                Debug.Assert(nativeType != IntPtr.Zero, $"{nameof(nativeType)} should be initialized");

                if (nativeType == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to get main program handle.");

                Photino_register_win32(nativeType);
            }
            else if (Platform.IsMacOS)
            {
                Photino_register_mac();
            }
            else if (Platform.IsLinux)
            {
                Photino_register_linux();
            }
            else
            {
                throw new PlatformNotSupportedException("PhotinoX supports Windows, macOS, and Linux.");
            }
        }
        catch
        {
            Interlocked.Exchange(ref s_initialized, 0);
            throw;
        }
    }
}