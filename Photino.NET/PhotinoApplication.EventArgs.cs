using System.ComponentModel;

namespace Photino.NET;

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.ShutdownRequested"/> event.
/// </summary>
/// <remarks>
/// Set <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/> to cancel the shutdown request.
/// On Windows, canceling a request with reason <see cref="PhotinoShutdownRequestReason.SessionLogoff"/>
/// or <see cref="PhotinoShutdownRequestReason.SystemShutdown"/> may prevent the user session from ending.
/// </remarks>
public sealed class ShutdownRequestedEventArgs : CancelEventArgs
{
    internal ShutdownRequestedEventArgs(PhotinoShutdownRequestReason reason)
    {
        Reason = reason;
    }

    /// <summary>
    /// Gets the reason for the shutdown request.
    /// </summary>
    public PhotinoShutdownRequestReason Reason { get; }
}

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
/// <param name="State">An optional user-defined state associated with the notification.</param>
public readonly record struct NotificationActivatedEventArgs(int NotificationId, object? State);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationActionActivated"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="ActionIndex">The activated notification action index.</param>
/// <param name="State">An optional user-defined state associated with the notification.</param>
public readonly record struct NotificationActionActivatedEventArgs(int NotificationId, int ActionIndex, object? State);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationInputActivated"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="Response">The notification input response.</param>
/// <param name="State">An optional user-defined state associated with the notification.</param>
public readonly record struct NotificationInputActivatedEventArgs(int NotificationId, string Response, object? State);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationDismissed"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="Reason">The notification dismissal reason.</param>
/// <param name="State">An optional user-defined state associated with the notification.</param>
public readonly record struct NotificationDismissedEventArgs(int NotificationId, NotificationDismissalReason Reason, object? State);

/// <summary>
/// Provides data for the <see cref="PhotinoApplication.NotificationFailed"/> event.
/// </summary>
/// <param name="NotificationId">The application notification correlation identifier.</param>
/// <param name="State">An optional user-defined state associated with the notification.</param>
public readonly record struct NotificationFailedEventArgs(int NotificationId, object? State);