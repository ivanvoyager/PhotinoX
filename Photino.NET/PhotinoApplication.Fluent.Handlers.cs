namespace Photino.NET;

partial class PhotinoApplication
{
    /// <summary>
    /// Registers a handler for application startup.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterStartupHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Startup += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for application exit.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterExitHandler(EventHandler<ExitEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Exit += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for application notification activation.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterNotificationActivatedHandler(EventHandler<PhotinoNotificationActivatedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        NotificationActivated += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for application notification action activation.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterNotificationActionActivatedHandler(EventHandler<PhotinoNotificationActionActivatedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        NotificationActionActivated += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for application notification input activation.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterNotificationInputActivatedHandler(EventHandler<PhotinoNotificationInputActivatedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        NotificationInputActivated += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for application notification dismissal.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterNotificationDismissedHandler(EventHandler<PhotinoNotificationDismissedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        NotificationDismissed += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for application notification failure.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterNotificationFailedHandler(EventHandler<PhotinoNotificationFailedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        NotificationFailed += handler;
        return this;
    }

    /// <summary>
    /// Registers a handler for dispatcher-level unhandled exceptions.
    /// </summary>
    /// <param name="handler">The handler to register.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoApplication RegisterDispatcherUnhandledExceptionHandler(UnhandledExceptionEventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        DispatcherUnhandledException += handler;
        return this;
    }
}