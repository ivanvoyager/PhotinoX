using System.ComponentModel;

namespace Photino.NET;

partial class PhotinoWindow
{
    /// <summary>
    /// Registers user-defined handler methods to receive callbacks before the native window is created.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterCreatingHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Creating += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks after the native window is created.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterCreatedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Created += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when the window is about to close.
    /// Set <see cref="CancelEventArgs.Cancel"/> to <c>true</c> to prevent the window from closing.
    /// </summary>
    /// <param name="handler">
    /// An <see cref="EventHandler{CancelEventArgs}"/> that can cancel the close operation.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterClosingHandler(EventHandler<CancelEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Closing += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks after the native window is closed.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterClosedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Closed += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when its location changes.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterLocationChangedHandler(EventHandler<LocationChangedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        LocationChanged += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when its size changes.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterSizeChangedHandler(EventHandler<SizeChangedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        SizeChanged += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the native window is activated.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterActivatedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Activated += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the native window is deactivated.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterDeactivatedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Deactivated += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it is maximized.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterMaximizedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Maximized += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it is restored.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterRestoredHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Restored += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it is minimized.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterMinimizedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        Minimized += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the native window enters fullscreen mode.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterFullScreenEnteredHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        FullScreenEntered += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the native window exits fullscreen mode.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterFullScreenExitedHandler(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        FullScreenExited += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the native window state changes.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterStateChangedHandler(EventHandler<StateChangedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        StateChanged += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the WebView content sends a message to the host application.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// Messages can be sent from JavaScript via <code>window.external.sendMessage(message)</code>.
    /// </remarks>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterWebMessageReceivedHandler(EventHandler<WebMessageReceivedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        WebMessageReceived += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks before the WebView starts navigating to top-level content.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// Set <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/> to cancel the navigation.
    /// </remarks>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterNavigationStartingHandler(EventHandler<NavigationStartingEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        NavigationStarting += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the WebView requests opening content in a new window.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// PhotinoX does not create browser-controlled popup windows. Applications can handle this event
    /// and open the requested URI externally if needed.
    /// </remarks>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterNewWindowRequestedHandler(EventHandler<NewWindowRequestedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        NewWindowRequested += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks when the WebView finishes loading top-level content.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// This event is raised for completed top-level content loads. It does not indicate
    /// that a JavaScript framework, SPA route, Blazor component tree, or all asynchronous
    /// page work has finished rendering.
    /// </remarks>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterContentLoadedHandler(EventHandler<ContentLoadedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        ContentLoaded += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive a callback when the initial top-level WebView content load completes.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// This event is raised only once, for the first completed top-level content load. It does not indicate
    /// that a JavaScript framework, SPA route, Blazor component tree, or all asynchronous
    /// page work has finished rendering.
    /// </remarks>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="handler"/> is <see langword="null"/>.
    /// </exception>
    public PhotinoWindow RegisterInitialContentLoadedHandler(EventHandler<ContentLoadedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ThrowIfClosed();
        InitialContentLoaded += handler;
        return this;
    }
}