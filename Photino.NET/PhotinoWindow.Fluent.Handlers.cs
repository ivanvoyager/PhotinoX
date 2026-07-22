using System.ComponentModel;
using System.Drawing;

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
    public PhotinoWindow RegisterCreatingHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterCreatedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterClosingHandler(EventHandler<CancelEventArgs>? handler)
    {
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
    public PhotinoWindow RegisterClosedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterLocationChangedHandler(EventHandler<Point>? handler)
    {
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
    public PhotinoWindow RegisterSizeChangedHandler(EventHandler<Size>? handler)
    {
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
    public PhotinoWindow RegisterActivatedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterDeactivatedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterMaximizedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterRestoredHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterMinimizedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterFullScreenEnteredHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterFullScreenExitedHandler(EventHandler? handler)
    {
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
    public PhotinoWindow RegisterStateChangedHandler(EventHandler<PhotinoWindowStateChangedEventArgs>? handler)
    {
        ThrowIfClosed();
        StateChanged += handler;
        return this;
    }

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it sends a message.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// Messages can be sent from JavaScript via <code>window.external.sendMessage(message)</code>
    /// </remarks>
    /// <param name="handler">The handler to register.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    public PhotinoWindow RegisterWebMessageReceivedHandler(EventHandler<string>? handler)
    {
        ThrowIfClosed();
        WebMessageReceived += handler;
        return this;
    }
}