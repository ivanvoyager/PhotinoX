namespace Photino.NET;

partial class PhotinoApplication
{
    /// <summary>
    /// Sets the application display name used by application-level native services.
    /// </summary>
    /// <param name="applicationName">The application display name.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called after the application has started.
    /// </exception>
    public PhotinoApplication SetName(string? applicationName)
    {
        Name = applicationName;
        return this;
    }

    /// <summary>
    /// Sets the application icon path used by application-level native services.
    /// </summary>
    /// <param name="iconPath">The application icon path.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called after the application has started.
    /// </exception>
    public PhotinoApplication SetIconPath(string? iconPath)
    {
        IconPath = iconPath;
        return this;
    }

    /// <summary>
    /// Sets the native notification registration identifier.
    /// </summary>
    /// <param name="registrationId">The native notification registration identifier.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when called after the application has started.
    /// </exception>
    public PhotinoApplication SetNotificationRegistrationId(string? registrationId)
    {
        NotificationRegistrationId = registrationId;
        return this;
    }

    /// <summary>
    /// Enables or disables application notifications.
    /// </summary>
    /// <param name="enabled"><see langword="true"/> to enable notifications; otherwise, <see langword="false"/>.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    public PhotinoApplication SetNotificationsEnabled(bool enabled)
    {
        NotificationsEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Sets the application shutdown mode.
    /// </summary>
    /// <param name="shutdownMode">The shutdown mode to use.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    public PhotinoApplication SetShutdownMode(PhotinoShutdownMode shutdownMode)
    {
        ShutdownMode = shutdownMode;
        return this;
    }
}