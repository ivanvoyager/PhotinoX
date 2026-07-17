using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

using InvokeCallback = NativeDelegates.VoidCallback;

internal static partial class NativeMethods
{
    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int PhotinoApplication_Run();

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void PhotinoApplication_Shutdown(int exitCode);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool PhotinoApplication_IsRunning();

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static partial bool PhotinoApplication_IsShuttingDown();

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool PhotinoApplication_Invoke(InvokeCallback callback);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool PhotinoApplication_BeginInvoke(InvokeCallback callback);

    internal static int RunNative()
    {
        int exitCode = PhotinoApplication_Run();
        Debug.Assert(exitCode == 0);
        return exitCode;
    }

    internal static bool InvokeNative(InvokeCallback callback)
    {
        if (callback == null)
            return false;

        return PhotinoApplication_Invoke(callback);
    }

    internal static bool BeginInvokeNative(InvokeCallback callback)
    {
        if (callback == null)
            return false;

        GCHandle handle = default;

        InvokeCallback wrapper = () =>
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                // TODO: Route asynchronous dispatcher exceptions through application-level unhandled exception handling.
                var message = $"Exception during asynchronous native dispatch: {ex}";
                Trace.WriteLine(message);
                Debug.Fail(message);
            }
            finally
            {
                handle.Free();
            }
        };

        handle = GCHandle.Alloc(wrapper);
        try
        {
            if (PhotinoApplication_BeginInvoke(wrapper))
            {
                // TODO: Release pending asynchronous callbacks if the native message loop exits before dispatching them.
                return true;
            }
        }
        catch
        {
            if (handle.IsAllocated)
                handle.Free();
            throw;
        }

        if (handle.IsAllocated)
            handle.Free();

        return false;
    }
}