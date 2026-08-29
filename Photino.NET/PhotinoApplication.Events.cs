using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Photino.NET;

partial class PhotinoApplication
{
    /// <summary>
    /// Occurs before the native application message loop starts.
    /// </summary>
    public event EventHandler? Startup;

    /// <summary>
    /// Invokes registered handlers before the native application message loop starts.
    /// </summary>
    /// <param name="state">The callback state.</param>
    internal void OnStartup(IntPtr state)
    {
        Debug.Assert(_notificationStates.IsEmpty);
        InvokeNativeEvent(Startup);
    }

    /// <summary>
    /// Occurs when application shutdown is requested.
    /// </summary>
    /// <remarks>
    /// Set <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/> to cancel the shutdown request.
    /// On Windows, canceling a request with reason <see cref="PhotinoShutdownRequestReason.SessionLogoff"/>
    /// or <see cref="PhotinoShutdownRequestReason.SystemShutdown"/> may prevent the user session from ending.
    /// </remarks>
    public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

    /// <summary>
    /// Invokes registered handlers when application shutdown is requested.
    /// </summary>
    /// <param name="reason">The reason for the shutdown request.</param>
    /// <param name="state">The callback state.</param>
    /// <returns>
    /// <c>1</c> to cancel shutdown; otherwise, <c>0</c>.
    /// </returns>
    internal byte OnShutdownRequested(PhotinoShutdownRequestReason reason, IntPtr state)
    {
        var handler = ShutdownRequested;
        if (handler is null)
            return 0;

        var args = new ShutdownRequestedEventArgs(reason);
        InvokeNativeEvent(handler, args);
        // C++ expects a single byte (0 = allow shutdown, 1 = cancel shutdown)
        return args.Cancel ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Occurs after the native application message loop exits.
    /// </summary>
    /// <remarks>
    /// Handlers can change <see cref="ExitEventArgs.ApplicationExitCode"/> to alter the value returned by
    /// <see cref="Run(PhotinoWindow?)"/>.
    /// </remarks>
    public event EventHandler<ExitEventArgs>? Exit;

    /// <summary>
    /// Invokes registered handlers after the native application message loop exits.
    /// </summary>
    /// <param name="exitCode">The native application exit code.</param>
    /// <param name="state">The callback state.</param>
    /// <returns>The application exit code after registered handlers have run.</returns>
    internal int OnExit(int exitCode, IntPtr state)
    {
        var args = new ExitEventArgs(exitCode);
        InvokeNativeEvent(Exit, args);
        return args.ApplicationExitCode;
    }

    /// <summary>
    /// Occurs when an application notification is activated.
    /// </summary>
    public event EventHandler<NotificationActivatedEventArgs>? NotificationActivated;

    /// <summary>
    /// Invokes registered handlers when an application notification is activated.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationActivated(int notificationId, IntPtr state)
    {
        InvokeNativeEvent(NotificationActivated, new NotificationActivatedEventArgs(notificationId, RemoveNotificationState(notificationId, state)));
    }

    /// <summary>
    /// Occurs when an application notification action is activated.
    /// </summary>
    public event EventHandler<NotificationActionActivatedEventArgs>? NotificationActionActivated;

    /// <summary>
    /// Invokes registered handlers when an application notification action is activated.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="actionIndex">The activated action index.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationActionActivated(int notificationId, int actionIndex, IntPtr state)
    {
        InvokeNativeEvent(NotificationActionActivated, new NotificationActionActivatedEventArgs(notificationId, actionIndex, RemoveNotificationState(notificationId, state)));
    }

    /// <summary>
    /// Occurs when an application notification input response is activated.
    /// </summary>
    public event EventHandler<NotificationInputActivatedEventArgs>? NotificationInputActivated;

    /// <summary>
    /// Invokes registered handlers when an application notification input response is activated.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="response">The notification input response.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationInputActivated(int notificationId, string response, IntPtr state)
    {
        InvokeNativeEvent(NotificationInputActivated, new NotificationInputActivatedEventArgs(notificationId, response, RemoveNotificationState(notificationId, state)));
    }

    /// <summary>
    /// Occurs when an application notification is dismissed.
    /// </summary>
    public event EventHandler<NotificationDismissedEventArgs>? NotificationDismissed;

    /// <summary>
    /// Invokes registered handlers when an application notification is dismissed.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="reason">The notification dismissal reason.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationDismissed(int notificationId, NotificationDismissalReason reason, IntPtr state)
    {
        InvokeNativeEvent(NotificationDismissed, new NotificationDismissedEventArgs(notificationId, reason, RemoveNotificationState(notificationId, state)));
    }

    /// <summary>
    /// Occurs when an application notification fails.
    /// </summary>
    public event EventHandler<NotificationFailedEventArgs>? NotificationFailed;

    /// <summary>
    /// Invokes registered handlers when an application notification fails.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationFailed(int notificationId, IntPtr state)
    {
        InvokeNativeEvent(NotificationFailed, new NotificationFailedEventArgs(notificationId, RemoveNotificationState(notificationId, state)));
    }

    private void InvokeNativeEvent<TEventArgs>(EventHandler<TEventArgs>? handler, TEventArgs args, [CallerMemberName] string? caller = null)
    {
        if (handler is null)
            return;

        try
        {
            handler(this, args);
        }
        catch (Exception ex)
        {
            HandleNativeCallbackException(ex, caller);
        }
    }

    private void InvokeNativeEvent(EventHandler? handler, [CallerMemberName] string? caller = null)
    {
        if (handler is null)
            return;

        try
        {
            handler(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            HandleNativeCallbackException(ex, caller);
        }
    }

    private void HandleNativeCallbackException(Exception exception, [CallerMemberName] string? caller = null)
    {
        try
        {
            Debug.Fail($"Unhandled exception in native callback '{caller}': {exception}");
            Dispatcher.OnUnhandledException(exception);
        }
        catch (Exception ex)
        {
            var message = $"Exception during dispatcher exception handling: {ex}";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }
    }
}