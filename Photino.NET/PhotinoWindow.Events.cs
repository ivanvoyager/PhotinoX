using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Photino.NET;

public partial class PhotinoWindow
{
    /// <summary>
    /// Occurs before the native window is created.
    /// </summary>
    public event EventHandler? Creating;

    /// <summary>
    /// Invokes registered handlers before the native window is created.
    /// </summary>
    internal void OnCreating()
    {
        Creating?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs after the native window has been created.
    /// </summary>
    public event EventHandler? Created;

    /// <summary>
    /// Invokes registered handlers after the native window is created.
    /// </summary>
    internal void OnCreated()
    {
        PhotinoApplication.Current.OnWindowCreated(this);
        Created?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the native window is about to close.
    /// </summary>
    /// <remarks>
    /// Set <see cref="CancelEventArgs.Cancel"/> to <c>true</c> to cancel the close operation.
    /// </remarks>
    public event EventHandler<CancelEventArgs>? Closing;

    /// <summary>
    /// Called by the native layer when the window is about to close.
    /// </summary>
    /// <returns>
    /// <c>1</c> to cancel closing; otherwise <c>0</c>.
    /// </returns>
    internal byte OnClosing()
    {
        if (_suppressClosing)
            return 0;

        var handler = Closing;
        if (handler == null)
            return 0;

        try
        {
            var args = new CancelEventArgs();
            handler(this, args);

            // C++ expects a single byte (0 = allow close, 1 = cancel close)
            return args.Cancel ? (byte)1 : (byte)0;
        }
        catch (Exception ex)
        {
            HandleNativeCallbackException(ex);
            return 0;
        }
    }

    /// <summary>
    /// Occurs after the native window is closed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Invokes registered handlers after the native window is closed.
    /// </summary>
    internal void OnClosed()
    {
        IsClosed = true;
        _nativeInstance = IntPtr.Zero;

        try
        {
            InvokeNativeEvent(Closed);
        }
        finally
        {
            PhotinoApplication.Current.OnWindowClosed(this);
        }
    }

    /// <summary>
    /// Occurs when the native window location changes.
    /// </summary>
    public event EventHandler<LocationChangedEventArgs>? LocationChanged;

    /// <summary>
    /// Invokes registered handlers when the native window location changes.
    /// </summary>
    /// <param name="left">The window position from the left in pixels.</param>
    /// <param name="top">The window position from the top in pixels.</param>
    internal void OnLocationChanged(int left, int top)
    {
        InvokeNativeEvent(LocationChanged, new LocationChangedEventArgs(new Point(left, top)));
    }

    /// <summary>
    /// Occurs when the native window size changes.
    /// </summary>
    public event EventHandler<SizeChangedEventArgs>? SizeChanged;

    /// <summary>
    /// Invokes registered handlers when the native window size changes.
    /// </summary>
    /// <param name="width">The window width in pixels.</param>
    /// <param name="height">The window height in pixels.</param>
    internal void OnSizeChanged(int width, int height)
    {
        InvokeNativeEvent(SizeChanged, new SizeChangedEventArgs(new Size(width, height)));
    }

    /// <summary>
    /// Occurs when the native window is activated.
    /// </summary>
    public event EventHandler? Activated;

    /// <summary>
    /// Invokes registered handlers when the native window is activated.
    /// </summary>
    internal void OnActivated()
    {
        InvokeNativeEvent(Activated);
    }

    /// <summary>
    /// Occurs when the native window is deactivated.
    /// </summary>
    public event EventHandler? Deactivated;

    /// <summary>
    /// Invokes registered handlers when the native window is deactivated.
    /// </summary>
    internal void OnDeactivated()
    {
        InvokeNativeEvent(Deactivated);
    }

    /// <summary>
    /// Occurs when the native window is maximized.
    /// </summary>
    public event EventHandler? Maximized;

    /// <summary>
    /// Invokes registered handlers when the native window is maximized.
    /// </summary>
    internal void OnMaximized()
    {
        InvokeNativeEvent(Maximized);
    }

    /// <summary>
    /// Occurs when the native window is restored to its normal state.
    /// </summary>
    public event EventHandler? Restored;

    /// <summary>
    /// Invokes registered handlers when the native window is restored.
    /// </summary>
    internal void OnRestored()
    {
        InvokeNativeEvent(Restored);
    }

    /// <summary>
    /// Occurs when the native window is minimized.
    /// </summary>
    public event EventHandler? Minimized;

    /// <summary>
    /// Invokes registered handlers when the native window is minimized.
    /// </summary>
    internal void OnMinimized()
    {
        InvokeNativeEvent(Minimized);
    }

    /// <summary>
    /// Occurs when the native window enters fullscreen mode.
    /// </summary>
    public event EventHandler? FullScreenEntered;

    /// <summary>
    /// Occurs when the native window exits fullscreen mode.
    /// </summary>
    public event EventHandler? FullScreenExited;

    /// <summary>
    /// Invokes registered handlers when the native fullscreen state changes.
    /// </summary>
    /// <param name="fullScreen">
    /// <see langword="true"/> when the native window enters fullscreen mode; otherwise, <see langword="false"/>.
    /// </param>
    internal void OnFullScreenChanged(bool fullScreen)
    {
        if (fullScreen)
            InvokeNativeEvent(FullScreenEntered);
        else
            InvokeNativeEvent(FullScreenExited);
    }

    /// <summary>
    /// Occurs when the native window state changes.
    /// </summary>
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Invokes registered handlers when the native window state changes.
    /// </summary>
    /// <param name="oldState">The previous native window state.</param>
    /// <param name="newState">The new native window state.</param>
    internal void OnStateChanged(PhotinoWindowState oldState, PhotinoWindowState newState)
    {
        InvokeNativeEvent(StateChanged, new StateChangedEventArgs(oldState, newState));
    }

    /// <summary>
    /// Occurs when the WebView content sends a message to the host application.
    /// </summary>
    public event EventHandler<WebMessageReceivedEventArgs>? WebMessageReceived;

    /// <summary>
    /// Invokes registered handlers when the WebView content sends a message to the host application.
    /// </summary>
    /// <param name="message">The message sent by the WebView content.</param>
    /// <param name="uri">The URI of the top-level WebView content at the time the message was received.</param>
    internal void OnWebMessageReceived(string message, string uri)
    {
        if (message is null)
        {
            Debug.Fail("Failed to receive message from WebView content: message is null");
            return;
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out var sourceUri))
        {
            Debug.Fail($"Failed to create URI from message source: {uri}");
            return;
        }

        InvokeNativeEvent(WebMessageReceived, new WebMessageReceivedEventArgs(message, sourceUri));
    }

    /// <summary>
    /// Occurs before the WebView starts navigating to top-level content.
    /// </summary>
    /// <remarks>
    /// Set <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/> to cancel the navigation.
    /// </remarks>
    public event EventHandler<NavigationStartingEventArgs>? NavigationStarting;

    /// <summary>
    /// Invokes registered handlers before the WebView starts navigating to top-level content.
    /// </summary>
    /// <param name="uri">The URI that the WebView is about to navigate to.</param>
    /// <returns>
    /// <c>1</c> to cancel navigation; otherwise, <c>0</c>.
    /// </returns>
    internal byte OnNavigationStarting(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var navigationUri))
        {
            Debug.Fail($"Failed to create URI from navigation target: {uri}");
            return 0;
        }

        var handler = NavigationStarting;
        if (handler is null)
            return 0;

        try
        {
            var args = new NavigationStartingEventArgs(navigationUri);
            handler(this, args);
            // C++ expects a single byte (0 = allow navigation, 1 = cancel navigation)
            return args.Cancel ? (byte)1 : (byte)0;
        }
        catch (Exception ex)
        {
            HandleNativeCallbackException(ex);
            return 0;
        }
    }

    /// <summary>
    /// Occurs when the WebView requests opening content in a new window.
    /// </summary>
    /// <remarks>
    /// PhotinoX does not create browser-controlled popup windows. Applications can handle this event
    /// and open the requested URI externally if needed.
    /// </remarks>
    public event EventHandler<NewWindowRequestedEventArgs>? NewWindowRequested;

    /// <summary>
    /// Invokes registered handlers when the WebView requests opening content in a new window.
    /// </summary>
    /// <param name="uri">The URI requested for the new window.</param>
    /// <returns>
    /// <c>1</c> to indicate that browser-controlled popup window creation must be suppressed.
    /// </returns>
    internal byte OnNewWindowRequested(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var requestedUri))
        {
            Debug.Fail($"Failed to create URI from new-window request: {uri}");
            return 1;
        }

        var handler = NewWindowRequested;
        if (handler is null)
            return 1;

        try
        {
            handler(this, new NewWindowRequestedEventArgs(requestedUri));

            // C++ expects a single byte (1 = request handled).
            // PhotinoX suppresses browser-controlled popup windows by default.
            return 1;
        }
        catch (Exception ex)
        {
            HandleNativeCallbackException(ex);
            return 1;
        }
    }

    /// <summary>
    /// Occurs when top-level WebView content starts loading after navigation has committed.
    /// </summary>
    /// <remarks>
    /// This event does not indicate that the document, JavaScript framework, SPA route,
    /// Blazor component tree, or asynchronous page work has finished loading or rendering.
    /// </remarks>
    public event EventHandler<ContentLoadingEventArgs>? ContentLoading;

    /// <summary>
    /// Invokes registered handlers when top-level WebView content starts loading.
    /// </summary>
    /// <param name="uri">The URI of the top-level WebView content that started loading.</param>
    internal void OnContentLoading(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var contentUri))
        {
            Debug.Fail($"Failed to create URI from content loading: {uri}");
            return;
        }

        OnContentLoading(contentUri);
    }

    /// <summary>
    /// Invokes registered handlers when top-level WebView content starts loading.
    /// </summary>
    /// <param name="uri">The URI of the top-level WebView content that started loading.</param>
    internal void OnContentLoading(Uri uri)
    {
        if (uri is null)
        {
            Debug.Fail("Failed to raise content loading event: URI is null");
            return;
        }

        InvokeNativeEvent(ContentLoading, new ContentLoadingEventArgs(uri));
    }

    /// <summary>
    /// Occurs when the WebView finishes loading top-level content.
    /// </summary>
    /// <remarks>
    /// This event is raised for completed top-level content loads. It does not indicate
    /// that a JavaScript framework, SPA route, Blazor component tree, or all asynchronous
    /// page work has finished rendering.
    /// </remarks>
    public event EventHandler<ContentLoadedEventArgs>? ContentLoaded;

    /// <summary>
    /// Occurs once when the initial top-level WebView content load completes.
    /// </summary>
    /// <remarks>
    /// This event does not indicate that a JavaScript framework, SPA route,
    /// Blazor component tree, or all asynchronous page work has finished rendering.
    /// </remarks>
    public event EventHandler<ContentLoadedEventArgs>? InitialContentLoaded;

    /// <summary>
    /// Invokes registered handlers when the WebView finishes loading top-level content.
    /// </summary>
    /// <param name="uri">The URI of the top-level WebView content that finished loading.</param>
    internal void OnContentLoaded(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var contentUri))
        {
            Debug.Fail($"Failed to create URI from content load: {uri}");
            return;
        }

        OnContentLoaded(contentUri);
    }

    private bool _firstContentLoadedRaised;

    /// <summary>
    /// Invokes registered handlers when the WebView finishes loading top-level content.
    /// </summary>
    /// <param name="uri">The URI of the top-level WebView content that finished loading.</param>
    internal void OnContentLoaded(Uri uri)
    {
        if (uri is null)
        {
            Debug.Fail("Failed to raise content load event: URI is null");
            return;
        }

        var args = new ContentLoadedEventArgs(uri);

        if (!_firstContentLoadedRaised)
        {
            _firstContentLoadedRaised = true;
            InvokeNativeEvent(InitialContentLoaded, args);
        }

        InvokeNativeEvent(ContentLoaded, args);
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
            Log($"Unhandled exception in native callback '{caller}': {exception}");
            Dispatcher.OnUnhandledException(exception);
        }
        catch (Exception ex)
        {
            // Never throw from native callback exception handling.
            var message = $"Exception during dispatcher exception handling: {ex}";
            Trace.WriteLine(message);
            Debug.Fail(message);
        }
    }
}
