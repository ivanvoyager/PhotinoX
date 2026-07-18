namespace Photino.NET;

partial class PhotinoDispatcher
{
    /// <summary>
    /// Gets the number of synchronous native dispatcher calls currently in progress.
    /// </summary>
    public int PendingInvokeCount => NativeMethods.PendingInvokeCount;

    /// <summary>
    /// Gets the number of asynchronous native dispatcher calls currently pending or executing.
    /// </summary>
    public int PendingBeginInvokeCount => NativeMethods.PendingBeginInvokeCount;

    /// <summary>
    /// Gets the number of failed synchronous native dispatcher scheduling attempts.
    /// </summary>
    public int InvokeFailureCount => NativeMethods.InvokeFailureCount;

    /// <summary>
    /// Gets the number of failed asynchronous native dispatcher scheduling attempts.
    /// </summary>
    public int BeginInvokeFailureCount => NativeMethods.BeginInvokeFailureCount;
}
