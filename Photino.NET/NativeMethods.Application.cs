using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

using InvokeCallback = NativeDelegates.VoidCallback;
using InvokeStateCallback = NativeDelegates.VoidStateCallback;

internal static partial class NativeMethods
{
    private static readonly InvokeStateCallback s_beginInvokeCallback = OnBeginInvoke;

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
    private static partial bool PhotinoApplication_BeginInvoke(InvokeStateCallback callback, IntPtr state);

    internal static int RunNative()
    {
        int exitCode = PhotinoApplication_Run();
        Debug.Assert(exitCode == 0);
        return exitCode;
    }

    internal static bool InvokeNative(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        Exception? exception = null;

        InvokeCallback wrapper = () =>
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        };

        bool result = PhotinoApplication_Invoke(wrapper);
        GC.KeepAlive(wrapper);

        if (exception is not null)
            ExceptionDispatchInfo.Capture(exception).Throw();

        return result;
    }

    internal static bool BeginInvokeNative(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var handle = GCHandle.Alloc(callback);
        try
        {
            if (PhotinoApplication_BeginInvoke(s_beginInvokeCallback, GCHandle.ToIntPtr(handle)))
            {
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

    private static void OnBeginInvoke(IntPtr state)
    {
        var handle = GCHandle.FromIntPtr(state);

        try
        {
            var callback = (Action)handle.Target!;
            callback();
        }
        catch (Exception ex)
        {
            try
            {
                PhotinoApplication.Current.OnDispatcherUnhandledException(ex);
            }
            catch (Exception handlerException)
            {
                var message = $"Exception during dispatcher exception handling: {handlerException}";
                Trace.WriteLine(message);
                Debug.Fail(message);
            }
        }
        finally
        {
            handle.Free();
        }
    }
}