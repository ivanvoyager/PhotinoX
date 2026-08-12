using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

using InvokeStateCallback = Photino.NET.NativeDelegates.VoidStateCallback;

namespace Photino.NET;

internal static partial class NativeMethods
{
    private sealed class InvokeState
    {
        public required Action Callback;
        public Exception? Exception;
    }

    private sealed class SendInvokeState
    {
        public required SendOrPostCallback Callback;
        public object? State;
        public Exception? Exception;
    }

    private sealed class PostInvokeState
    {
        public required SendOrPostCallback Callback;
        public object? State;
    }

    private static readonly InvokeStateCallback s_invokeCallback = OnInvoke;
    private static readonly InvokeStateCallback s_invokeStateCallback = OnInvokeState;
    private static readonly InvokeStateCallback s_postCallback = OnPost;
    private static readonly InvokeStateCallback s_postStateCallback = OnPostState;

    private static int s_invokeCount;
    private static int s_beginInvokeCount;
    private static int s_invokeFailureCount;
    private static int s_beginInvokeFailureCount;

    internal static int PendingInvokeCount => Volatile.Read(ref s_invokeCount);
    internal static int PendingBeginInvokeCount => Volatile.Read(ref s_beginInvokeCount);
    internal static int InvokeFailureCount => Volatile.Read(ref s_invokeFailureCount);
    internal static int BeginInvokeFailureCount => Volatile.Read(ref s_beginInvokeFailureCount);

#pragma warning disable SYSLIB1054
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int PhotinoApplication_Run(ref PhotinoApplicationNativeParameters parameters);
#pragma warning restore SYSLIB1054

    [LibraryImport(DLL_NAME)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void PhotinoApplication_Shutdown(int exitCode, byte force);

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

    #region Sync helpers

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

    internal static bool InvokeNative(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var invokeState = new SendInvokeState
        {
            Callback = callback,
            State = state
        };

        var handle = GCHandle.Alloc(invokeState);

        bool result;

        Interlocked.Increment(ref s_invokeCount);
        try
        {
            result = PhotinoApplication_Invoke(s_invokeStateCallback, GCHandle.ToIntPtr(handle));
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

    private static void OnInvokeState(IntPtr value)
    {
        Debug.Assert(value != IntPtr.Zero);
        if (value == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(value);
        var state = (SendInvokeState)handle.Target!;

        try
        {
            state.Callback(state.State);
        }
        catch (Exception ex)
        {
            state.Exception = ex;
        }
    }

    #endregion

    #region Async helpers

    internal static bool BeginInvokeNative(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var handle = GCHandle.Alloc(callback);
        Interlocked.Increment(ref s_beginInvokeCount);
        try
        {
            if (PhotinoApplication_BeginInvoke(s_postCallback, GCHandle.ToIntPtr(handle)))
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

    private static void OnPost(IntPtr state)
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

    internal static bool BeginInvokeNative(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var invokeState = new PostInvokeState
        {
            Callback = callback,
            State = state
        };

        var handle = GCHandle.Alloc(invokeState);
        Interlocked.Increment(ref s_beginInvokeCount);
        try
        {
            if (PhotinoApplication_BeginInvoke(s_postStateCallback, GCHandle.ToIntPtr(handle)))
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

    private static void OnPostState(IntPtr value)
    {
        Debug.Assert(value != IntPtr.Zero);
        if (value == IntPtr.Zero)
            return;

        var handle = GCHandle.FromIntPtr(value);
        try
        {
            var state = (PostInvokeState)handle.Target!;
            state.Callback(state.State);
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

    #endregion
}