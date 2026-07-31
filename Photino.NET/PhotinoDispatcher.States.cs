using System.Runtime.CompilerServices;

namespace Photino.NET;

partial class PhotinoDispatcher
{
    private sealed class InvokeActionState<TState>(Action<TState> callback, TState state)
    {
        public readonly Action<TState> Callback = callback;
        public readonly TState State = state;
    }

    private sealed class InvokeFuncState<TResult>(Func<TResult> callback)
    {
        public readonly Func<TResult> Callback = callback;
        public TResult Result = default!;
    }

    private sealed class InvokeFuncState<TState, TResult>(Func<TState, TResult> callback, TState state)
    {
        public readonly Func<TState, TResult> Callback = callback;
        public readonly TState State = state;
        public TResult Result = default!;
    }

    private sealed class InvokeAsyncActionState(Action callback, CancellationToken cancellationToken)
    {
        internal readonly Action Callback = callback;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                Callback();
                Completion.TrySetResult();
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncState(SendOrPostCallback callback, object? state, CancellationToken cancellationToken)
    {
        internal readonly SendOrPostCallback Callback = callback;
        internal readonly object? State = state;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                Callback(State);
                Completion.TrySetResult();
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncActionState<TState>(Action<TState> callback, TState state, CancellationToken cancellationToken)
    {
        internal readonly Action<TState> Callback = callback;
        internal readonly TState State = state;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                Callback(State);
                Completion.TrySetResult();
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncFuncState<TResult>(Func<TResult> callback, CancellationToken cancellationToken)
    {
        internal readonly Func<TResult> Callback = callback;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource<TResult> Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource<TResult>)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                TResult result = Callback();
                Completion.TrySetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncFuncState<TState, TResult>(Func<TState, TResult> callback, TState state, CancellationToken cancellationToken)
    {
        internal readonly Func<TState, TResult> Callback = callback;
        internal readonly TState State = state;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource<TResult> Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource<TResult>)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                TResult result = Callback(State);
                Completion.TrySetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncValueTaskState(Func<CancellationToken, ValueTask> callback, CancellationToken cancellationToken)
    {
        internal readonly Func<CancellationToken, ValueTask> Callback = callback;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal async void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                await Callback(CancellationToken).ConfigureAwait(false);
                Completion.TrySetResult();
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncValueTaskState<TState>(Func<TState, CancellationToken, ValueTask> callback, TState state, CancellationToken cancellationToken)
    {
        internal readonly Func<TState, CancellationToken, ValueTask> Callback = callback;
        internal readonly TState State = state;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal async void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                await Callback(State, CancellationToken).ConfigureAwait(false);
                Completion.TrySetResult();
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncValueTaskResultState<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken)
    {
        internal readonly Func<CancellationToken, ValueTask<TResult>> Callback = callback;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource<TResult> Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource<TResult>)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal async void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                TResult result = await Callback(CancellationToken).ConfigureAwait(false);
                Completion.TrySetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }

    private sealed class InvokeAsyncValueTaskResultState<TState, TResult>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken)
    {
        internal readonly Func<TState, CancellationToken, ValueTask<TResult>> Callback = callback;
        internal readonly TState State = state;
        internal readonly CancellationToken CancellationToken = cancellationToken;

        public readonly TaskCompletionSource<TResult> Completion = new (TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration CancellationRegistration;

        internal void RegisterCancellation()
        {
            if (!CancellationToken.CanBeCanceled)
                return;

            CancellationRegistration = CancellationToken.UnsafeRegister(
                static (state, cancellationToken) => ((TaskCompletionSource<TResult>)state!).TrySetCanceled(cancellationToken),
                Completion);
        }

        internal async void Execute()
        {
            CancellationRegistration.Dispose();

            if (Completion.Task.IsCompleted || Completion.TrySetCanceledIfRequested(CancellationToken))
                return;

            try
            {
                TResult result = await Callback(State, CancellationToken).ConfigureAwait(false);
                Completion.TrySetResult(result);
            }
            catch (OperationCanceledException ex) when (ex.IsCancellationRequested(CancellationToken))
            {
                Completion.TrySetCanceled(CancellationToken);
            }
            catch (Exception ex)
            {
                Completion.TrySetException(ex);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Fail(Exception exception)
        {
            CancellationRegistration.Dispose();
            Completion.TrySetException(exception);
        }
    }
}