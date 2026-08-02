using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

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

        var args = new CancelEventArgs();
        handler(this, args);

        // C++ expects a single byte (0 = allow close, 1 = cancel close)
        return args.Cancel ? (byte)1 : (byte)0;
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
            Closed?.Invoke(this, EventArgs.Empty);
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
        LocationChanged?.Invoke(this, new LocationChangedEventArgs(new Point(left, top)));
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
        SizeChanged?.Invoke(this, new SizeChangedEventArgs(new Size(width, height)));
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
        Activated?.Invoke(this, EventArgs.Empty);
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
        Deactivated?.Invoke(this, EventArgs.Empty);
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
        Maximized?.Invoke(this, EventArgs.Empty);
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
        Restored?.Invoke(this, EventArgs.Empty);
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
        Minimized?.Invoke(this, EventArgs.Empty);
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
            FullScreenEntered?.Invoke(this, EventArgs.Empty);
        else
            FullScreenExited?.Invoke(this, EventArgs.Empty);
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
        StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, newState));
    }

    /// <summary>
    /// Occurs when the native window sends a message to the host application.
    /// </summary>
    public event EventHandler<WebMessageReceivedEventArgs>? WebMessageReceived;

    /// <summary>
    /// Invokes registered handlers when the WebView content sends a message to the host application.
    /// </summary>
    /// <param name="message">The message sent by the WebView content.</param>
    /// <param name="uri">The URI of the top-level WebView content at the time the message was received.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="message"/> or <paramref name="uri"/> is <see langword="null"/>.
    /// </exception>
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

        WebMessageReceived?.Invoke(this, new WebMessageReceivedEventArgs(message, sourceUri));
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
    /// Invokes registered handlers when the WebView finishes loading top-level window content.
    /// </summary>
    /// <param name="uri">The URI of the top-level WebView content that finished loading.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="uri"/> is <see langword="null"/>.
    /// </exception>
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
    /// Invokes registered handlers when the WebView finishes loading top-level window content.
    /// </summary>
    /// <param name="uri">The URI of the top-level WebView content that finished loading.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="uri"/> is <see langword="null"/>.
    /// </exception>
    internal void OnContentLoaded(Uri uri)
    {
        var args = new ContentLoadedEventArgs(uri);

        if (!_firstContentLoadedRaised)
        {
            _firstContentLoadedRaised = true;
            InitialContentLoaded?.Invoke(this, args);
        }

        ContentLoaded?.Invoke(this, args);
    }

    // TODO: Handle exceptions thrown by event handlers and log them to the console or a logger.
}
