using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using static Photino.NET.NativeMethods;

namespace Photino.NET;

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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This method does not throw for dispatcher scheduling failures. Scheduling failures are reported through diagnostics and dispatcher statistics.
    /// Exceptions thrown by <paramref name="callback"/> are reported through <see cref="UnhandledException"/>.
    /// </remarks>
    public bool BeginInvoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return BeginInvokeNative(callback);
    }

    /// <summary>
    /// Posts the specified <see cref="SendOrPostCallback"/> to the dispatcher thread and returns immediately.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The object passed to the callback.</param>
    /// <returns><c>true</c> if the callback was scheduled; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This method does not throw for dispatcher scheduling failures. Scheduling failures are reported through diagnostics and dispatcher statistics.
    /// Exceptions thrown by <paramref name="callback"/> are reported through <see cref="UnhandledException"/>.
    /// </remarks>
    public bool Post(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        return BeginInvokeNative(callback, state);
    }

    /// <summary>
    /// Executes the specified <see cref="Action"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The action to execute.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the callback cannot be scheduled on the dispatcher thread.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// </remarks>
    public void Invoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback();
            return;
        }

        bool success = InvokeNative(callback);
        Debug.Assert(success);

        if (!success)
            throw CreateFailedException();
    }

    /// <summary>
    /// Attempts to execute the specified <see cref="Action"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The action to execute.</param>
    /// <returns><c>true</c> if the callback was executed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// Dispatcher scheduling failures are returned as <c>false</c> and reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool TryInvoke(Action callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback();
            return true;
        }

        return InvokeNative(callback);
    }

    /// <summary>
    /// Executes the specified state-based <see cref="Action{T}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <param name="callback">The action to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the callback cannot be scheduled on the dispatcher thread.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// </remarks>
    public void Invoke<TState>(Action<TState> callback, TState state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback(state);
            return;
        }

        var invokeState = new InvokeActionState<TState>(callback, state);

        bool success = InvokeNative(static value =>
        {
            var state = (InvokeActionState<TState>)value!;
            state.Callback(state.State);
        }, invokeState);

        Debug.Assert(success);

        if (!success)
            throw CreateFailedException();
    }

    /// <summary>
    /// Attempts to execute the specified state-based <see cref="Action{T}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <param name="callback">The action to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <returns><c>true</c> if the callback was executed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// Dispatcher scheduling failures are returned as <c>false</c> and reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool TryInvoke<TState>(Action<TState> callback, TState state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback(state);
            return true;
        }

        var invokeState = new InvokeActionState<TState>(callback, state);

        bool success = InvokeNative(static value =>
        {
            var state = (InvokeActionState<TState>)value!;
            state.Callback(state.State);
        }, invokeState);

        return success;
    }

    /// <summary>
    /// Executes the specified <see cref="Func{TResult}"/> on the dispatcher thread and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <returns>The value returned by <paramref name="callback"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the callback cannot be scheduled on the dispatcher thread.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// </remarks>
    public TResult Invoke<TResult>(Func<TResult> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
            return callback();

        var state = new InvokeFuncState<TResult>(callback: callback);

        bool success = InvokeNative(static value =>
        {
            var state = (InvokeFuncState<TResult>)value!;
            state.Result = state.Callback();
        }, state);

        Debug.Assert(success);

        if (!success)
            throw CreateFailedException();

        return state.Result;
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// Dispatcher scheduling failures are returned as <c>false</c> and reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool TryInvoke<TResult>(Func<TResult> callback, out TResult result)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            result = callback();
            return true;
        }

        var state = new InvokeFuncState<TResult>(callback);

        bool success = InvokeNative(static value =>
        {
            var state = (InvokeFuncState<TResult>)value!;
            state.Result = state.Callback();
        }, state);

        result = state.Result;
        return success;
    }

    /// <summary>
    /// Executes the specified state-based <see cref="Func{T,TResult}"/> on the dispatcher thread and returns its result.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <returns>The value returned by <paramref name="callback"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the callback cannot be scheduled on the dispatcher thread.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// </remarks>
    public TResult Invoke<TState, TResult>(Func<TState, TResult> callback, TState state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
            return callback(state);

        var invokeState = new InvokeFuncState<TState, TResult>(callback, state);

        bool success = InvokeNative(static value =>
        {
            var state = (InvokeFuncState<TState, TResult>)value!;
            state.Result = state.Callback(state.State);
        }, invokeState);

        Debug.Assert(success);

        if (!success)
            throw CreateFailedException();

        return invokeState.Result;
    }

    /// <summary>
    /// Attempts to execute the specified state-based <see cref="Func{T,TResult}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <param name="result">
    /// When this method returns <c>true</c>, contains the value returned by <paramref name="callback"/>.
    /// Otherwise, contains the default value of <typeparamref name="TResult"/>.
    /// </param>
    /// <returns><c>true</c> if the callback was executed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// Dispatcher scheduling failures are returned as <c>false</c> and reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool TryInvoke<TState, TResult>(Func<TState, TResult> callback, TState state, out TResult result)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            result = callback(state);
            return true;
        }

        var invokeState = new InvokeFuncState<TState, TResult>(callback, state);

        bool success = InvokeNative(static value =>
        {
            var state = (InvokeFuncState<TState, TResult>)value!;
            state.Result = state.Callback(state.State);
        }, invokeState);

        result = invokeState.Result;
        return success;
    }

    /// <summary>
    /// Executes the specified <see cref="SendOrPostCallback"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The object passed to <paramref name="callback"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the callback cannot be scheduled on the dispatcher thread.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// </remarks>
    public void Send(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback(state);
            return;
        }

        bool success = InvokeNative(callback, state);
        Debug.Assert(success);

        if (!success)
            throw CreateFailedException();
    }

    /// <summary>
    /// Attempts to execute the specified <see cref="SendOrPostCallback"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The object passed to <paramref name="callback"/>.</param>
    /// <returns><c>true</c> if the callback was executed; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by <paramref name="callback"/> are propagated to the caller.
    /// Dispatcher scheduling failures are returned as <c>false</c> and reported through diagnostics and dispatcher statistics.
    /// </remarks>
    public bool TrySend(SendOrPostCallback callback, object? state)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (CheckAccess())
        {
            callback(state);
            return true;
        }

        return InvokeNative(callback, state);
    }

    /// <summary>
    /// Asynchronously executes the specified <see cref="Action"/> on the dispatcher thread.
    /// </summary>
    /// <param name="callback">The action to execute.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the action has finished executing, is canceled, or faults if the action cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by the callback are captured by the returned task.
    /// </remarks>
    public Task InvokeAsync(Action callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var invokeState = new InvokeAsyncActionState(callback, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncActionState)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Posts the specified <see cref="SendOrPostCallback"/> to the dispatcher thread and returns a task for its completion.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The object passed to the callback.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the posted callback has finished executing, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by the posted callback are captured by the returned task.
    /// </remarks>
    public Task PostAsync(SendOrPostCallback callback, object? state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var invokeState = new InvokeAsyncState(callback, state, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncState)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified state-based <see cref="Action{T}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <param name="callback">The action to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the action has finished executing, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by the callback are captured by the returned task.
    /// </remarks>
    public Task InvokeAsync<TState>(Action<TState> callback, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var invokeState = new InvokeAsyncActionState<TState>(callback, state, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncActionState<TState>)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified <see cref="Func{TResult}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes when the function has finished executing, is canceled, or faults if the function cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by the callback are captured by the returned task.
    /// </remarks>
    public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<TResult>(cancellationToken);

        var invokeState = new InvokeAsyncFuncState<TResult>(callback, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncFuncState<TResult>)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified state-based <see cref="Func{T,TResult}"/> on the dispatcher thread.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The function to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes when the function has finished executing, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by the callback are captured by the returned task.
    /// </remarks>
    public Task<TResult> InvokeAsync<TState, TResult>(Func<TState, TResult> callback, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<TResult>(cancellationToken);

        var invokeState = new InvokeAsyncFuncState<TState, TResult>(callback, state, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncFuncState<TState, TResult>)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified asynchronous callback through the dispatcher.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the operation returned by <paramref name="callback"/> completes, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by the callback or by the operation it returns are captured by the returned task.
    /// </remarks>
    public Task InvokeAsync(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var invokeState = new InvokeAsyncValueTaskState(callback, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncValueTaskState)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified state-based asynchronous callback through the dispatcher.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the operation returned by <paramref name="callback"/> completes, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by the callback or by the operation it returns are captured by the returned task.
    /// </remarks>
    public Task InvokeAsync<TState>(Func<TState, CancellationToken, ValueTask> callback, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var invokeState = new InvokeAsyncValueTaskState<TState>(callback, state, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncValueTaskState<TState>)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified asynchronous callback through the dispatcher and returns its result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes when the operation returned by <paramref name="callback"/> completes, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// Exceptions thrown by the callback or by the operation it returns are captured by the returned task.
    /// </remarks>
    public Task<TResult> InvokeAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<TResult>(cancellationToken);

        var invokeState = new InvokeAsyncValueTaskResultState<TResult>(callback, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncValueTaskResultState<TResult>)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
    }

    /// <summary>
    /// Asynchronously executes the specified state-based asynchronous callback through the dispatcher and returns its result.
    /// </summary>
    /// <typeparam name="TState">The callback state type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="callback">The callback to execute.</param>
    /// <param name="state">The state passed to <paramref name="callback"/>.</param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes when the operation returned by <paramref name="callback"/> completes, is canceled, or faults if the callback cannot be scheduled.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="callback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    /// <remarks>
    /// This overload allows callers to pass state explicitly and avoid closure allocations.
    /// Exceptions thrown by the callback or by the operation it returns are captured by the returned task.
    /// </remarks>
    public Task<TResult> InvokeAsync<TState, TResult>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(callback);

        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<TResult>(cancellationToken);

        var invokeState = new InvokeAsyncValueTaskResultState<TState, TResult>(callback, state, cancellationToken);
        invokeState.RegisterCancellation();

        bool success = BeginInvokeNative(static value =>
        {
            var state = (InvokeAsyncValueTaskResultState<TState, TResult>)value!;
            state.Execute();
        }, invokeState);

        if (!success)
            invokeState.Fail(CreateFailedException());

        return invokeState.Completion.Task;
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

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCurrentThreadDoesNotHaveDispatcherAccess()
    {
        throw new InvalidOperationException("The current thread does not have access to the dispatcher.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowMustBeCreatedOnDispatcherThread()
    {
        throw new InvalidOperationException("Photino windows must be created on the dispatcher thread.");
    }
}
