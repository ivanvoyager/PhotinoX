namespace Photino.NET;

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.Exit"/> event.
/// </summary>
public sealed class ExitEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExitEventArgs"/> class.
    /// </summary>
    /// <param name="applicationExitCode">The application exit code returned by <see cref="PhotinoApplication.Run(PhotinoWindow?)"/>.</param>
    public ExitEventArgs(int applicationExitCode)
    {
        ApplicationExitCode = applicationExitCode;
    }

    /// <summary>
    /// Gets or sets the application exit code returned by <see cref="PhotinoApplication.Run(PhotinoWindow?)"/>.
    /// </summary>
    public int ApplicationExitCode { get; set; }
}

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationActivated"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
public readonly record struct PhotinoNotificationActivatedEventArgs(int NotificationId);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationActionActivated"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="ActionIndex">The activated notification action index.</param>
public readonly record struct PhotinoNotificationActionActivatedEventArgs(int NotificationId, int ActionIndex);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationInputActivated"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="Response">The notification input response.</param>
public readonly record struct PhotinoNotificationInputActivatedEventArgs(int NotificationId, string Response);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationDismissed"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="Reason">The notification dismissal reason.</param>
public readonly record struct PhotinoNotificationDismissedEventArgs(int NotificationId, PhotinoNotificationDismissalReason Reason);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationFailed"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
public readonly record struct PhotinoNotificationFailedEventArgs(int NotificationId);