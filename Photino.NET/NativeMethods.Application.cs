using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

using InvokeStateCallback = NativeDelegates.VoidStateCallback;

internal static partial class NativeMethods
{
    private sealed class InvokeState
    {
        public required Action Callback;
        public Exception? Exception;
    }

    private sealed class SendOrPostInvokeState
    {
        public required SendOrPostCallback Callback;
        public object? State;
        public Exception? Exception;
    }

    private static readonly InvokeStateCallback s_invokeCallback = OnInvoke;
    private static readonly InvokeStateCallback s_beginInvokeCallback = OnBeginInvoke;
    private static readonly InvokeStateCallback s_sendOrPostInvokeCallback = OnSendOrPostInvoke;
    private static readonly InvokeStateCallback s_sendOrPostBeginInvokeCallback = OnSendOrPostBeginInvoke;

    private static int s_invokeCount;
    private static int s_beginInvokeCount;
    private static int s_invokeFailureCount;
    private static int s_beginInvokeFailureCount;

    internal static int PendingInvokeCount => Volatile.Read(ref s_invokeCount);
    internal static int PendingBeginInvokeCount => Volatile.Read(ref s_beginInvokeCount);
    internal static int InvokeFailureCount => Volatile.Read(ref s_invokeFailureCount);
    internal static int BeginInvokeFailureCount => Volatile.Read(ref s_beginInvokeFailureCount);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial int PhotinoApplication_Run();

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
    internal static partial bool PhotinoApplication_CheckAccess();

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool PhotinoApplication_Invoke(InvokeStateCallback callback, IntPtr state);

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool PhotinoApplication_BeginInvoke(InvokeStateCallback callback, IntPtr state);

    internal static bool InvokeNative(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var state = new InvokeState { Callback = callback };
        var handle = GCHandle.Alloc(state);

        bool result;

        Interlocked.Increment(ref s_invokeCount);
        try
        {
            result = PhotinoApplication_Invoke(s_invokeCallback, GCHandle.ToIntPtr(handle));
        }
        catch
        {
            Interlocked.Increment(ref s_invokeFailureCount);
            throw;
        }
        finally
        {
            Interlocked.Decrement(ref s_invokeCount);
            handle.Free();
        }

        if (!result)
            Interlocked.Increment(ref s_invokeFailureCount);

        if (state.Exception is not null)
            ExceptionDispatchInfo.Capture(state.Exception).Throw();

        return result;
    }

    private static void OnInvoke(IntPtr value)
    {
        Debug.Assert(value != IntPtr.Zero);
        if (value == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(value);
        var state = (InvokeState)handle.Target!;

        try
        {
            state.Callback();
        }
        catch (Exception ex)
        {
            state.Exception = ex;
        }
    }

    internal static bool BeginInvokeNative(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var handle = GCHandle.Alloc(callback);
        Interlocked.Increment(ref s_beginInvokeCount);
        try
        {
            if (PhotinoApplication_BeginInvoke(s_beginInvokeCallback, GCHandle.ToIntPtr(handle)))
            {
                return true;
            }
            Interlocked.Increment(ref s_beginInvokeFailureCount);
        }
        catch
        {
            Interlocked.Increment(ref s_beginInvokeFailureCount);
            if (handle.IsAllocated)
            {
                Interlocked.Decrement(ref s_beginInvokeCount);
                handle.Free();
            }
            throw;
        }

        if (handle.IsAllocated)
        {
            Interlocked.Decrement(ref s_beginInvokeCount);
            handle.Free();
        }

        return false;
    }

    private static void OnBeginInvoke(IntPtr state)
    {
        Debug.Assert(state != IntPtr.Zero);
        if (state == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(state);
        try
        {
            var callback = (Action)handle.Target!;
            callback();
        }
        catch (Exception ex)
        {
            OnDispatcherUnhandledException(ex);
        }
        finally
        {
            Interlocked.Decrement(ref s_beginInvokeCount);
            handle.Free();
        }
    }

    internal static bool InvokeNative(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var invokeState = new SendOrPostInvokeState
        {
            Callback = callback,
            State = state
        };

        var handle = GCHandle.Alloc(invokeState);

        bool result;

        Interlocked.Increment(ref s_invokeCount);
        try
        {
            result = PhotinoApplication_Invoke(s_sendOrPostInvokeCallback, GCHandle.ToIntPtr(handle));
        }
        catch
        {
            Interlocked.Increment(ref s_invokeFailureCount);
            throw;
        }
        finally
        {
            Interlocked.Decrement(ref s_invokeCount);
            handle.Free();
        }

        if (!result)
            Interlocked.Increment(ref s_invokeFailureCount);

        if (invokeState.Exception is not null)
            ExceptionDispatchInfo.Capture(invokeState.Exception).Throw();

        return result;
    }

    private static void OnSendOrPostInvoke(IntPtr value)
    {
        Debug.Assert(value != IntPtr.Zero);
        if (value == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(value);
        var state = (SendOrPostInvokeState)handle.Target!;

        try
        {
            state.Callback(state.State);
        }
        catch (Exception ex)
        {
            state.Exception = ex;
        }
    }

    internal static bool BeginInvokeNative(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var handle = GCHandle.Alloc((callback, state));
        Interlocked.Increment(ref s_beginInvokeCount);
        try
        {
            if (PhotinoApplication_BeginInvoke(s_sendOrPostBeginInvokeCallback, GCHandle.ToIntPtr(handle)))
            {
                return true;
            }
            Interlocked.Increment(ref s_beginInvokeFailureCount);
        }
        catch
        {
            Interlocked.Increment(ref s_beginInvokeFailureCount);
            if (handle.IsAllocated)
            {
                Interlocked.Decrement(ref s_beginInvokeCount);
                handle.Free();
            }
            throw;
        }

        if (handle.IsAllocated)
        {
            Interlocked.Decrement(ref s_beginInvokeCount);
            handle.Free();
        }

        return false;
    }

    private static void OnSendOrPostBeginInvoke(IntPtr value)
    {
        Debug.Assert(value != IntPtr.Zero);
        if (value == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(value);
        try
        {
            (SendOrPostCallback callback, object? state) = ((SendOrPostCallback, object?))handle.Target!;
            callback(state);
        }
        catch (Exception ex)
        {
            OnDispatcherUnhandledException(ex);
        }
        finally
        {
            Interlocked.Decrement(ref s_beginInvokeCount);
            handle.Free();
        }
    }

    internal static void OnDispatcherUnhandledException(Exception ex)
    {
        try
        {
            PhotinoApplication.Current.Dispatcher.OnUnhandledException(ex);
        }
        catch (Exception handlerException)
        {
            var message = $"Exception during dispatcher exception handling: {handlerException}";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }
    }
}