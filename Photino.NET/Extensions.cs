using System.Runtime.CompilerServices;

namespace Photino.NET;

internal static class OperationCanceledExceptionExtensions
{
    extension(OperationCanceledException exception)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCancellationRequested(CancellationToken cancellationToken)
        {
            return exception.CancellationToken == cancellationToken || cancellationToken.IsCancellationRequested;
        }
    }
}

internal static class TaskCompletionSourceExtensions
{
    extension(TaskCompletionSource taskCompletionSource)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetCanceledIfRequested(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            taskCompletionSource.TrySetCanceled(cancellationToken);
            return true;
        }
    }

    extension<TResult>(TaskCompletionSource<TResult> taskCompletionSource)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySetCanceledIfRequested(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            taskCompletionSource.TrySetCanceled(cancellationToken);
            return true;
        }
    }
}