namespace Photino.NET;

partial class PhotinoDispatcher
{
    private sealed class InvokeActionState<TState>
    {
        public required Action<TState> Callback;
        public TState State = default!;
    }

    private sealed class InvokeFuncState<TResult>
    {
        public required Func<TResult> Callback;
        public TResult Result = default!;
    }

    private sealed class InvokeFuncState<TState, TResult>
    {
        public required Func<TState, TResult> Callback;
        public TState State = default!;
        public TResult Result = default!;
    }

    private sealed class InvokeAsyncState
    {
        public required TaskCompletionSource Completion;
        public required SendOrPostCallback Callback;
        public object? State;
    }

    private sealed class InvokeAsyncActionState
    {
        public required TaskCompletionSource Completion;
        public required Action Callback;
    }

    private sealed class InvokeAsyncActionState<TState>
    {
        public required TaskCompletionSource Completion;
        public required Action<TState> Callback;
        public TState State = default!;
    }

    private sealed class InvokeAsyncFuncState<TResult>
    {
        public required TaskCompletionSource<TResult> Completion;
        public required Func<TResult> Callback;
    }

    private sealed class InvokeAsyncFuncState<TState, TResult>
    {
        public required TaskCompletionSource<TResult> Completion;
        public required Func<TState, TResult> Callback;
        public TState State = default!;
    }

    private sealed class InvokeAsyncTaskState
    {
        public required TaskCompletionSource Completion;
        public required Func<Task> Callback;
    }

    private sealed class InvokeAsyncTaskState<TResult>
    {
        public required TaskCompletionSource<TResult> Completion;
        public required Func<Task<TResult>> Callback;
    }
}
