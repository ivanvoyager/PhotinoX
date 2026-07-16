using System.ComponentModel;
using System.Drawing;

namespace Photino.NET;

public partial class PhotinoWindow
{
    /// <summary>
    /// Occurs when the native window location changes.
    /// </summary>
    public event EventHandler<Point>? WindowLocationChanged;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when its location changes.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterLocationChangedHandler(EventHandler<Point>? handler)
    {
        WindowLocationChanged += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window's location changes.
    /// </summary>
    /// <param name="left">Position from left in pixels</param>
    /// <param name="top">Position from top in pixels</param>
    internal void OnLocationChanged(int left, int top)
    {
        var location = new Point(left, top);
        WindowLocationChanged?.Invoke(this, location);
    }
    
    /// <summary>
    /// Occurs when the native window size changes.
    /// </summary>
    public event EventHandler<Size>? WindowSizeChanged;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when its size changes.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterSizeChangedHandler(EventHandler<Size>? handler)
    {
        WindowSizeChanged += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window's size changes.
    /// </summary>
    internal void OnSizeChanged(int width, int height)
    {
        var size = new Size(width, height);
        WindowSizeChanged?.Invoke(this, size);
    }

    /// <summary>
    /// Occurs when the native window receives input focus.
    /// </summary>
    public event EventHandler? WindowFocusIn;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window
    /// when it receives input focus.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterFocusInHandler(EventHandler? handler)
    {
        WindowFocusIn += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window focuses in.
    /// </summary>
    internal void OnFocusIn()
    {
        WindowFocusIn?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the native window is maximized.
    /// </summary>
    public event EventHandler? WindowMaximized;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it is maximized.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterMaximizedHandler(EventHandler? handler)
    {
        WindowMaximized += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window is maximized.
    /// </summary>
    internal void OnMaximized()
    {
        WindowMaximized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the native window is restored from a minimized or maximized state.
    /// </summary>
    public event EventHandler? WindowRestored;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it is restored.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterRestoredHandler(EventHandler? handler)
    {
        WindowRestored += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window is restored.
    /// </summary>
    internal void OnRestored()
    {
        WindowRestored?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the native window loses input focus.
    /// </summary>
    public event EventHandler? WindowFocusOut;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it loses input focus.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterFocusOutHandler(EventHandler? handler)
    {
        WindowFocusOut += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window focuses out.
    /// </summary>
    internal void OnFocusOut()
    {
        WindowFocusOut?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the native window is minimized.
    /// </summary>
    public event EventHandler? WindowMinimized;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it is minimized.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterMinimizedHandler(EventHandler? handler)
    {
        WindowMinimized += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window is minimized.
    /// </summary>
    internal void OnMinimized()
    {
        WindowMinimized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs when the native window sends a message to the host application.
    /// </summary>
    public event EventHandler<string>? WebMessageReceived;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks from the native window when it sends a message.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// Messages can be sent from JavaScript via <code>window.external.sendMessage(message)</code>
    /// </remarks>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterWebMessageReceivedHandler(EventHandler<string>? handler)
    {
        WebMessageReceived += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods when the native window sends a message.
    /// </summary>
    internal void OnWebMessageReceived(string message)
    {
        WebMessageReceived?.Invoke(this, message);
    }

    /// <summary>
    /// Occurs when the native window is about to close.
    /// </summary>
    /// <remarks>
    /// Set <see cref="CancelEventArgs.Cancel"/> to <c>true</c> to cancel the close operation.
    /// </remarks>
    public event EventHandler<CancelEventArgs>? WindowClosing;

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
    public PhotinoWindow RegisterWindowClosingHandler(EventHandler<CancelEventArgs>? handler)
    {
        WindowClosing += handler;
        return this;
    }

    /// <summary>
    /// Called by the native layer when the window is about to close.
    /// </summary>
    /// <returns>
    /// <c>1</c> to cancel closing; otherwise <c>0</c>.
    /// </returns>
    internal byte OnWindowClosing()
    {
        var handler = WindowClosing;
        if (handler == null)
            return 0;

        var args = new CancelEventArgs();
        handler?.Invoke(this, args);

        // C++ expects a single byte (0 = allow close, 1 = cancel close)
        return args.Cancel ? (byte)1 : (byte)0;
    }

    /// <summary>
    /// Occurs after the native window is closed.
    /// </summary>
    public event EventHandler? WindowClosed;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks after the native window is closed.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterWindowClosedHandler(EventHandler? handler)
    {
        WindowClosed += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods after the native window is closed.
    /// </summary>
    internal void OnWindowClosed()
    {
        try
        {
            WindowClosed?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _nativeInstance = IntPtr.Zero;
            Interlocked.Exchange(ref _managedThreadId, 0);
        }
    }

    /// <summary>
    /// Occurs before the native window is created.
    /// </summary>
    public event EventHandler? WindowCreating;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks before the native window is created.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterWindowCreatingHandler(EventHandler? handler)
    {
        WindowCreating += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods before the native window is created.
    /// </summary>
    internal void OnWindowCreating()
    {
        WindowCreating?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Occurs after the native window has been created.
    /// </summary>
    public event EventHandler? WindowCreated;

    /// <summary>
    /// Registers user-defined handler methods to receive callbacks after the native window is created.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="handler"><see cref="EventHandler"/></param>
    public PhotinoWindow RegisterWindowCreatedHandler(EventHandler? handler)
    {
        WindowCreated += handler;
        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods after the native window is created.
    /// </summary>
    internal void OnWindowCreated()
    {
        WindowCreated?.Invoke(this, EventArgs.Empty);
    }
}
