using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

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

    //NOTE: There is 1 callback from C++ to C# which is automatically registered. The .NET callback appropriate for the custom scheme is handled in OnCustomScheme().

    /// <summary>
    /// Provides a response stream for a user-defined custom URI scheme.
    /// </summary>
    /// <param name="sender">The <see cref="PhotinoWindow"/> instance.</param>
    /// <param name="scheme">The scheme portion of the requested URL.</param>
    /// <param name="url">The full request URL.</param>
    /// <param name="contentType">
    /// The MIME content type of the response; may be <c>null</c>.
    /// </param>
    /// <returns>
    /// A readable <see cref="Stream"/> containing the response data, or <c>null</c>
    /// to indicate that the request should be handled by the default browser logic.
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Stream"/> is consumed synchronously and will be disposed
    /// by the framework after the response is read.
    /// </remarks>
    public delegate Stream? NetCustomSchemeDelegate(object? sender, string scheme, string url, out string? contentType);

    /// <summary>
    /// Stores registered custom scheme handlers keyed by scheme name.
    /// Multiple handlers for the same scheme are aggregated.
    /// </summary>
    internal Dictionary<string, NetCustomSchemeDelegate> CustomSchemes = [];

    /// <summary>
    /// Registers user-defined custom schemes (other than 'http', 'https' and 'file') and handler methods to receive callbacks
    /// when the native browser control encounters them.
    /// </summary>
    /// <remarks>
    /// Up to 16 unique custom scheme names can be registered before native initialization.
    /// After initialization, additional handlers may be added for existing schemes.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="scheme">The custom scheme</param>
    /// <param name="handler"><see cref="NetCustomSchemeDelegate"/></param>
    /// <exception cref="ArgumentException">Thrown if no scheme or handler was provided</exception>
    /// <exception cref="InvalidOperationException">Thrown if more than 16 custom schemes were set</exception>
    public PhotinoWindow RegisterCustomSchemeHandler(string scheme, NetCustomSchemeDelegate handler)
    {
        if (string.IsNullOrWhiteSpace(scheme)) throw new ArgumentException("A scheme must be provided (for example 'app' or 'custom').", nameof(scheme));

        _ = handler ?? throw new ArgumentException("A handler (method) with a signature matching NetCustomSchemeDelegate must be supplied.", nameof(handler));

        scheme = scheme.ToLowerInvariant();

        if (_nativeInstance == IntPtr.Zero)
        {
            if (!CustomSchemes.TryGetValue(scheme, out var existing))
            {
                if (CustomSchemes.Count >= 16)
                    throw new InvalidOperationException($"No more than 16 custom schemes can be set prior to initialization. Additional handlers can be added after initialization.");

                CustomSchemes[scheme] = handler;
            }
            else
            {
                CustomSchemes[scheme] = existing + handler;
            }
        }
        else
        {
            Photino_AddCustomSchemeName(_nativeInstance, scheme);

            if (CustomSchemes.TryGetValue(scheme, out var existing))
                CustomSchemes[scheme] = existing + handler;
            else
                CustomSchemes[scheme] = handler;
        }

        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods for custom URI schemes (other than 'http','https', and 'file')
    /// when the native browser control encounters them.
    /// </summary>
    /// <param name="url">URL of the Scheme</param>
    /// <param name="numBytes">Number of bytes of the response</param>
    /// <param name="contentType">Content type of the response</param>
    /// <returns><see cref="IntPtr"/></returns>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty or consists only of white-space characters.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the URL does not contain a colon.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered.
    /// </exception>
    public IntPtr OnCustomScheme(string url, out int numBytes, out string? contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        var colonPos = url.IndexOf(':');

        if (colonPos < 0)
            throw new ArgumentException($"URL: '{url}' does not contain a colon.", nameof(url));

        var scheme = url.Substring(0, colonPos).ToLowerInvariant();

        if (!CustomSchemes.TryGetValue(scheme, out NetCustomSchemeDelegate? handler))
            throw new InvalidOperationException($"A handler for the custom scheme '{scheme}' has not been registered.");

        var responseStream = handler.Invoke(this, scheme, url, out contentType);

        if (responseStream == null)
        {
            // Webview should pass through request to normal handlers (e.g., network)
            // or handle as 404 otherwise
            numBytes = 0;
            return IntPtr.Zero;
        }

        // Read the stream into memory and serve the bytes
        // In the future, it would be possible to pass the stream through into C++
        using (responseStream)
        using (var ms = new MemoryStream())
        {
            responseStream.CopyTo(ms);

            numBytes = (int)ms.Position;
            // Memory allocated here should be released by the native layer after the response is processed.
            var buffer = Marshal.AllocHGlobal(numBytes);
            Marshal.Copy(ms.GetBuffer(), 0, buffer, numBytes);
            //_hGlobalToFree.Add(buffer);
            return buffer;
        }
    }
}
