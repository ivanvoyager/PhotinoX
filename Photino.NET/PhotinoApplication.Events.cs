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
    internal void OnStartup()
    {
        InvokeNativeEvent(Startup);
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
    /// <returns>The application exit code after registered handlers have run.</returns>
    internal int OnExit(int exitCode)
    {
        var args = new ExitEventArgs(exitCode);
        InvokeNativeEvent(Exit, args);
        return args.ApplicationExitCode;
    }

    /// <summary>
    /// Occurs when an application notification is activated.
    /// </summary>
    public event EventHandler<PhotinoNotificationActivatedEventArgs>? NotificationActivated;

    /// <summary>
    /// Invokes registered handlers when an application notification is activated.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationActivated(int notificationId, IntPtr state)
    {
        InvokeNativeEvent(NotificationActivated, new PhotinoNotificationActivatedEventArgs(notificationId));
    }

    /// <summary>
    /// Occurs when an application notification action is activated.
    /// </summary>
    public event EventHandler<PhotinoNotificationActionActivatedEventArgs>? NotificationActionActivated;

    /// <summary>
    /// Invokes registered handlers when an application notification action is activated.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="actionIndex">The activated action index.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationActionActivated(int notificationId, int actionIndex, IntPtr state)
    {
        InvokeNativeEvent(NotificationActionActivated, new PhotinoNotificationActionActivatedEventArgs(notificationId, actionIndex));
    }

    /// <summary>
    /// Occurs when an application notification input response is activated.
    /// </summary>
    public event EventHandler<PhotinoNotificationInputActivatedEventArgs>? NotificationInputActivated;

    /// <summary>
    /// Invokes registered handlers when an application notification input response is activated.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="response">The notification input response.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationInputActivated(int notificationId, string response, IntPtr state)
    {
        InvokeNativeEvent(NotificationInputActivated, new PhotinoNotificationInputActivatedEventArgs(notificationId, response));
    }

    /// <summary>
    /// Occurs when an application notification is dismissed.
    /// </summary>
    public event EventHandler<PhotinoNotificationDismissedEventArgs>? NotificationDismissed;

    /// <summary>
    /// Invokes registered handlers when an application notification is dismissed.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="reason">The notification dismissal reason.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationDismissed(int notificationId, PhotinoNotificationDismissalReason reason, IntPtr state)
    {
        InvokeNativeEvent(NotificationDismissed, new PhotinoNotificationDismissedEventArgs(notificationId, reason));
    }

    /// <summary>
    /// Occurs when an application notification fails.
    /// </summary>
    public event EventHandler<PhotinoNotificationFailedEventArgs>? NotificationFailed;

    /// <summary>
    /// Invokes registered handlers when an application notification fails.
    /// </summary>
    /// <param name="notificationId">The application notification correlation identifier.</param>
    /// <param name="state">The notification state.</param>
    internal void OnNotificationFailed(int notificationId, IntPtr state)
    {
        InvokeNativeEvent(NotificationFailed, new PhotinoNotificationFailedEventArgs(notificationId));
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