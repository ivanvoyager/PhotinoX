using System.Diagnostics;

namespace Photino.NET;

using static NativeMethods;

/// <summary>
/// Provides access to the Photino application dispatcher.
/// </summary>
/// <remarks>
/// The dispatcher can be accessed before the application message loop starts,
/// but dispatch operations can only be scheduled while the application is running.
/// </remarks>
public sealed partial class PhotinoDispatcher
{
    private int _threadId;

    /// <summary>
    /// Provides notifications of unhandled exceptions that occur within the dispatcher.
    /// </summary>
    public event UnhandledExceptionEventHandler? UnhandledException;

    /// <summary>
    /// Returns a value that indicates whether the current thread has access to the dispatcher.
    /// </summary>
    /// <returns><c>true</c> if the current thread has dispatcher access; otherwise, <c>false</c>.</returns>
    public bool CheckAccess()
    {
        int threadId = Volatile.Read(ref _threadId);

        if (threadId == Environment.CurrentManagedThreadId)
            return true;

        return PhotinoApplication_CheckAccess();
    }

    /// <summary>
    /// Verifies that the current thread has access to the dispatcher.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The current thread does not have access to the dispatcher.
    /// </exception>
    public void VerifyAccess()
    {
        if (!CheckAccess())
            ThrowCurrentThreadDoesNotHaveDispatcherAccess();
    }

    /// <summary>
    /// Verifies that the current thread is the dispatcher thread used for native window creation.
    /// </summary>
    /// <remarks>
    /// The first native window creation binds the dispatcher to the current managed thread.
    /// All subsequent native windows must be created on the same thread.
    /// </remarks>
    internal void VerifyAccessToCreateWindow()
    {
        int currentThreadId = Environment.CurrentManagedThreadId;

        int threadId = Volatile.Read(ref _threadId);
        if (threadId == currentThreadId)
            return;

        if (threadId == 0 && Interlocked.CompareExchange(ref _threadId, currentThreadId, 0) == 0)
        {
            return;
        }

        ThrowWindowMustBeCreatedOnDispatcherThread();
    }

    /// <summary>
    /// Posts the specified <see cref="Action"/> to the dispatcher thread and returns immediately.
    /// </summary>
    /// <param name="callback">The action to execute.</param>
    /// <returns><c>true</c> if the callback was scheduled; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not throw if the callback cannot be scheduled. Scheduling failures are reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool BeginInvoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        bool success = BeginInvokeNative(callback);
        Debug.Assert(success);
        return success;
    }

    /// <summary>
    /// Posts the specified <see cref="SendOrPostCallback"/> to the dispatcher thread and returns immediately.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The object passed to the callback.</param>
    /// <returns><c>true</c> if the callback was scheduled; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method does not throw if the callback cannot be scheduled. Scheduling failures are reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool BeginInvoke(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        bool success = BeginInvokeNative(callback, state);
        Debug.Assert(success);
        return success;
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The action to execute.</param>
    /// <returns><c>true</c> if the callback was executed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller. Dispatcher scheduling failures are reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool Invoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback();
            return true;
        }

        bool success = InvokeNative(callback);
        Debug.Assert(success);
        return success;
    }

    /// <summary>
    /// Attempts to execute the specified <see cref="Func{TResult}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <param name="result">
    /// When this method returns <c>true</c>, contains the value returned by <paramref name="callback"/>.
    /// Otherwise, contains the default value of <typeparamref name="TResult"/>.
    /// </param>
    /// <returns><c>true</c> if the callback was executed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// Dispatcher scheduling failures are reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool TryInvoke<TResult>(Func<TResult> callback, out TResult result)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            result = callback();
            return true;
        }

        TResult localResult = default!;
        bool success = InvokeNative(() => localResult = callback());
        Debug.Assert(success);

        result = localResult;
        return success;
    }

    /// <summary>
    /// Asynchronously executes the specified <see cref="SendOrPostCallback"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The object passed to the callback.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the callback has finished executing, or faults if the callback cannot be scheduled.
    /// </returns>
    public Task InvokeAsync(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool success = BeginInvokeNative(static x =>
        {
            (TaskCompletionSource<bool> tcs, SendOrPostCallback callback, object? state) = ((TaskCompletionSource<bool>, SendOrPostCallback, object?))x!;
            try
            {
                callback(state);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, (tcs, callback, state));
        Debug.Assert(success);
        if (!success)
            tcs.SetException(CreateFailedException());

        return tcs.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified <see cref="Action"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The action to execute.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the action has finished executing, or faults if the action cannot be scheduled.
    /// </returns>
    public Task InvokeAsync(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool success = BeginInvokeNative(static x =>
        {
            (TaskCompletionSource<bool> tcs, Action callback) = ((TaskCompletionSource<bool>, Action))x!;
            try
            {
                callback();
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, (tcs, callback));
        Debug.Assert(success);
        if (!success)
            tcs.SetException(CreateFailedException());

        return tcs.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified <see cref="Func{TResult}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes when the function has finished executing, or faults if the function cannot be scheduled.
    /// </returns>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool success = BeginInvokeNative(static x =>
        {
            (TaskCompletionSource<TResult> tcs, Func<TResult> callback) = ((TaskCompletionSource<TResult>, Func<TResult>))x!;
            try
            {
                TResult result = callback();
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, (tcs, callback));
        Debug.Assert(success);
        if (!success)
            tcs.SetException(CreateFailedException());

        return tcs.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified task-returning callback through the dispatcher.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the task returned by <paramref name="callback"/> completes, or faults if the callback cannot be scheduled.
    /// </returns>
    public Task InvokeAsync(Func<Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool success = BeginInvokeNative(async static x =>
        {
            (TaskCompletionSource<bool> tcs, Func<Task> callback) = ((TaskCompletionSource<bool>, Func<Task>))x!;
            try
            {
                await callback().ConfigureAwait(false);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, (tcs, callback));
        Debug.Assert(success);
        if (!success)
            tcs.SetException(CreateFailedException());

        return tcs.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified task-returning callback through the dispatcher and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The callback to execute.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes when the task returned by <paramref name="callback"/> completes, or faults if the callback cannot be scheduled.
    /// </returns>
    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        bool success = BeginInvokeNative(async static x =>
        {
            (TaskCompletionSource<TResult> tcs, Func<Task<TResult>> callback) = ((TaskCompletionSource<TResult>, Func<Task<TResult>>))x!;
            try
            {
                TResult result = await callback().ConfigureAwait(false);
                tcs.SetResult(result);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, (tcs, callback));
        Debug.Assert(success);
        if (!success)
            tcs.SetException(CreateFailedException());

        return tcs.Task;
    }

    internal void OnUnhandledException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var handler = UnhandledException;
        if (handler is not null)
        {
            var args = new UnhandledExceptionEventArgs(exception, false);
            handler(this, args);
        }

        TraceUnhandledException(exception);

        return;

        static void TraceUnhandledException(Exception exception)
        {
            var message = $"Unhandled dispatcher exception: {exception}";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }
    }

    private static InvalidOperationException CreateFailedException()
    {
        return new InvalidOperationException("Failed to schedule the callback on the dispatcher thread.");
    }

    private static void ThrowCurrentThreadDoesNotHaveDispatcherAccess()
    {
        throw new InvalidOperationException("The current thread does not have access to the dispatcher.");
    }

    private static void ThrowWindowMustBeCreatedOnDispatcherThread()
    {
        throw new InvalidOperationException("Photino windows must be created on the dispatcher thread.");
    }
}
