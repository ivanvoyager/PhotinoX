using System.Runtime.InteropServices;

using static Photino.NET.NativeMethods;

namespace Photino.NET;

partial class PhotinoApplication
{
    private static int NewNotificationId
    {
        get
        {
            while (true)
            {
                // Read current, compute next, normalize to positive range
                int current = Volatile.Read(ref field);
                int next = current + 1;
                if (next <= 0) next = 1;

                // CAS: if no one changed field since 'current', publish 'next'
                if (Interlocked.CompareExchange(ref field, next, current) == current)
                    return next;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether application notifications are enabled.
    /// </summary>
    /// <remarks>
    /// Before the application starts, this value is stored as startup configuration and passed to the native layer
    /// when <see cref="Run(PhotinoWindow?)"/> starts. After the application has started, setting this property
    /// updates the native notification state.
    /// </remarks>
    public bool NotificationsEnabled
    {
        get
        {
            if (!IsRunning)
                return _startupParameters.NotificationsEnabled;

            return Dispatcher.Invoke(static () =>
            {
                PhotinoApplication_GetNotificationsEnabled(out byte enabled);
                return enabled != 0;
            });
        }
        set
        {
            _startupParameters.NotificationsEnabled = value;

            if (!IsRunning)
                return;

            Dispatcher.Send(static state =>
            {
                PhotinoApplication_SetNotificationsEnabled((byte)((bool)state! ? 1 : 0));
            }, value);
        }
    }

    /// <summary>
    /// Gets or sets the native notification registration identifier.
    /// </summary>
    /// <remarks>
    /// On Windows, this value is used as the application notification registration identifier.
    /// On Linux, this value is passed as the notification desktop-entry hint.
    /// The value is passed to the native application layer when <see cref="Run(PhotinoWindow?)"/> starts.
    /// It cannot be changed after the application has started.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when setting the value after the application has started.
    /// </exception>
    public string? NotificationRegistrationId
    {
        get => _startupParameters.NotificationRegistrationId;
        set
        {
            ThrowIfRunning();

            _startupParameters.NotificationRegistrationId = value;
        }
    }

    /// <summary>
    /// Shows an application notification.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="body">The notification body.</param>
    /// <param name="iconPath">
    /// The optional notification icon path. If not specified, the application icon path is used.
    /// </param>
    /// <returns>
    /// A positive application notification correlation identifier if the notification was accepted for display;
    /// <c>0</c> if the notification was not shown; or a negative value if the native notification request failed.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="title"/> or <paramref name="body"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="title"/> or <paramref name="body"/> is empty or whitespace.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the application is not running.
    /// </exception>
    public int ShowNotification(string title, string body, string? iconPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        if (!IsRunning)
            ThrowApplicationNotRunning();

        if (!NotificationsEnabled)
            return 0;

        var showParams = new PhotinoNotificationShowNativeParameters
        {
            Size = Marshal.SizeOf<PhotinoNotificationShowNativeParameters>(),
            AbiVersion = PhotinoNotificationShowNativeParameters.NativeAbiVersion,
            NotificationId = NewNotificationId,
            Title = title,
            Body = body,
            IconPath = string.IsNullOrWhiteSpace(iconPath) ? IconPath : iconPath,
            CallbackState = IntPtr.Zero
        };

        return Dispatcher.Invoke(static parameters => PhotinoApplication_ShowNotification(ref parameters), showParams);
    }
}