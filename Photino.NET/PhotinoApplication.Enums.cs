namespace Photino.NET;

/// <summary>
/// Specifies when a <see cref="PhotinoApplication"/> shuts down.
/// </summary>
public enum PhotinoShutdownMode
{
    /// <summary>
    /// The application shuts down when the last application window closes.
    /// </summary>
    OnLastWindowClose,

    /// <summary>
    /// The application shuts down when the main window closes.
    /// </summary>
    OnMainWindowClose,

    /// <summary>
    /// The application shuts down only when <see cref="PhotinoApplication.Shutdown(int, bool)"/> is called.
    /// </summary>
    OnExplicitShutdown,
}

internal static class PhotinoShutdownModeExtensions
{
    extension(PhotinoShutdownMode shutdownMode)
    {
        internal bool IsValid() => shutdownMode is PhotinoShutdownMode.OnLastWindowClose or PhotinoShutdownMode.OnMainWindowClose or PhotinoShutdownMode.OnExplicitShutdown;
    }
}

/// <summary>
/// Specifies why application shutdown was requested.
/// </summary>
public enum PhotinoShutdownRequestReason
{
    /// <summary>
    /// The native platform did not provide a specific shutdown request reason.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Shutdown was requested by the application.
    /// </summary>
    Application = 1,

    /// <summary>
    /// Shutdown was requested because the user session is ending due to logoff.
    /// </summary>
    SessionLogoff = 2,

    /// <summary>
    /// Shutdown was requested because the system is shutting down or restarting.
    /// </summary>
    SystemShutdown = 3
}

/// <summary>
/// Specifies why an application notification was dismissed.
/// </summary>
public enum NotificationDismissalReason
{
    /// <summary>
    /// The native platform did not provide a specific dismissal reason.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The notification was dismissed by the user.
    /// </summary>
    UserCanceled = 1,

    /// <summary>
    /// The notification was dismissed because the application was hidden or deactivated by the platform.
    /// </summary>
    ApplicationHidden = 2,

    /// <summary>
    /// The notification timed out.
    /// </summary>
    TimedOut = 3
}