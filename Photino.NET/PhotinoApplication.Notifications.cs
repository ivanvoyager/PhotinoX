using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

using static Photino.NET.NativeMethods;

namespace Photino.NET;

partial class PhotinoApplication
{
    private readonly ConcurrentDictionary<int, IntPtr> _notificationStates = new();

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
    /// <param name="state">
    /// An optional user-defined state associated with the notification.
    /// The value is passed back when a notification callback is raised.
    /// </param>
    /// <returns>
    /// A positive application notification correlation identifier if the notification request was accepted;
    /// <c>0</c> if the notification was not shown by policy or application state;
    /// <c>-1</c> if the request was invalid or could not be tracked;
    /// <c>-2</c> if native notification backend initialization failed;
    /// <c>-3</c> if native notification display failed.
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
    public int ShowNotification(string title, string body, string? iconPath = null, object? state = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        if (!IsRunning || IsShuttingDown)
            ThrowApplicationNotRunning();

        if (!NotificationsEnabled)
            return 0;

        int notificationId = NewNotificationId;
        var callbackState = state is not null ? GCHandle.ToIntPtr(GCHandle.Alloc(state)) : IntPtr.Zero;

        var showParams = new PhotinoNotificationShowNativeParameters
        {
            Size = Marshal.SizeOf<PhotinoNotificationShowNativeParameters>(),
            AbiVersion = PhotinoNotificationShowNativeParameters.NativeAbiVersion,
            NotificationId = notificationId,
            Title = title,
            Body = body,
            IconPath = string.IsNullOrWhiteSpace(iconPath) ? IconPath : iconPath,
            CallbackState = callbackState
        };

        if (callbackState != IntPtr.Zero && !_notificationStates.TryAdd(notificationId, callbackState))
        {
            GCHandle.FromIntPtr(callbackState).Free();
            return -1;
        }

        int result;
        try
        {
            result = Dispatcher.Invoke(static parameters => PhotinoApplication_ShowNotification(ref parameters), showParams);
        }
        catch (Exception)
        {
            if (callbackState != IntPtr.Zero)
                FreeNotificationState(notificationId, callbackState);
            throw;
        }

        if (result <= 0 && callbackState != IntPtr.Zero)
        {
            FreeNotificationState(notificationId, callbackState);
        }

        return result;
    }

    private void FreeNotificationState(int notificationId, IntPtr expectedState)
    {
        if (!_notificationStates.TryRemove(notificationId, out var trackedState))
            return;

        Debug.Assert(trackedState == expectedState);
        GCHandle.FromIntPtr(trackedState).Free();
    }

    private void ClearNotificationStates()
    {
        if (_notificationStates.IsEmpty)
            return;

        Debug.WriteLine($"PhotinoX: Clearing notification states: {_notificationStates.Count}");
        int cleared = 0;
        int count = _notificationStates.Count;
        foreach (var pair in _notificationStates)
        {
            if (_notificationStates.TryRemove(pair.Key, out var handlePtr))
            {
                Debug.WriteLineIf(cleared < 10, $"PhotinoX: Clearing notification state: {pair.Key}");
                GCHandle.FromIntPtr(handlePtr).Free();
                cleared++;
            }
        }
        Debug.WriteLineIf(cleared > 10, $"PhotinoX: Clearing notification states ... +{cleared - 10} items");
        Debug.WriteLineIf(cleared != count, $"PhotinoX: Inconsistent notification state counts: {cleared}/{count}");
        Debug.WriteLineIf(cleared == count, $"PhotinoX: Cleared notification states: {cleared}/{count}");
    }

    private object? RemoveNotificationState(int notificationId, IntPtr state)
    {
        if (state == IntPtr.Zero)
            return null;

        if (!_notificationStates.TryRemove(notificationId, out var trackedState))
            return null;

        Debug.Assert(trackedState == state);

        if (trackedState != state)
        {
            GCHandle.FromIntPtr(trackedState).Free();
            return null;
        }

        var handle = GCHandle.FromIntPtr(state);

        try
        {
            return handle.Target;
        }
        finally
        {
            handle.Free();
        }
    }
}