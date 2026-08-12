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
    /// The application shuts down only when <see cref="PhotinoApplication.Shutdown(int)"/> is called.
    /// </summary>
    OnExplicitShutdown,
}

/// <summary>
/// Specifies why an application notification was dismissed.
/// </summary>
public enum PhotinoNotificationDismissalReason
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