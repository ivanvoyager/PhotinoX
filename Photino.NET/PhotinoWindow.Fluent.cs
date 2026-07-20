using System.Drawing;
using Photino.NET.Utils;

namespace Photino.NET;

using static NativeMethods;

partial class PhotinoWindow
{
    #region Lifecycle / window actions

    /// <summary>
    /// Maximizes the native window.
    /// </summary>
    /// <remarks>
    /// If called before native window initialization, the window will be maximized on startup.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow Maximize()
    {
        Log($".{nameof(Maximize)}()");
        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
        {
            _startupParameters.Maximized = true;
            _startupParameters.Minimized = false;
            _startupParameters.FullScreen = false;
        }
        else
        {
            Invoke(() => Photino_Maximize(_nativeInstance));
        }

        return this;
    }

    /// <summary>
    /// Minimizes the native window.
    /// </summary>
    /// <remarks>
    /// If called before native window initialization, the window will be minimized on startup.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow Minimize()
    {
        Log($".{nameof(Minimize)}()");
        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
        {
            _startupParameters.Minimized = true;
            _startupParameters.Maximized = false;
            _startupParameters.FullScreen = false;
        }
        else
        {
            Invoke(() => Photino_Minimize(_nativeInstance));
        }

        return this;
    }

    /// <summary>
    /// Restores the native window from a minimized, maximized, or fullscreen state back to its normal state.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow Restore()
    {
        Log($".{nameof(Restore)}()");
        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
        {
            _startupParameters.Minimized = false;
            _startupParameters.Maximized = false;
            _startupParameters.FullScreen = false;
        }
        else
        {
            Invoke(() => Photino_Restore(_nativeInstance));
        }

        return this;
    }

    /// <summary>
    /// Brings the native Photino window to the front.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// If the window has not been created yet, it is created first.
    /// If the window is minimized, it is restored before activation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    public PhotinoWindow BringToFront()
    {
        Log($".{nameof(BringToFront)}()");
        ThrowIfClosed();

        Show();

        byte minimized = 0;
        Invoke(() => Photino_GetMinimized(_nativeInstance, out minimized));
        if (minimized != 0)
            Restore();

        Activate();
        return this;
    }

    #endregion

    #region Startup / initialization options

    /// <summary>
    /// Sets the native window <see cref="PhotinoWindow.Title"/>.
    /// Default is <c>PhotinoX</c>.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="title">Window title</param>
    public PhotinoWindow SetTitle(string title)
    {
        Log($".{nameof(SetTitle)}({title})");
        Title = title;
        return this;
    }

    /// <summary>
    /// Sets the icon file for the native window title bar.
    /// The file must be located on the local machine and cannot be a URL. The default is none.
    /// </summary>
    /// <remarks>
    /// This only works on Windows and Linux.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="iconFile"/> is null, empty, whitespace, or does not reference an existing file.
    /// </exception>
    /// <param name="iconFile">The file path to the icon.</param>
    public PhotinoWindow SetIconFile(string iconFile)
    {
        Log($".{nameof(SetIconFile)}({iconFile})");
        IconFile = iconFile;
        return this;
    }

    /// <summary>
    /// When true the native window starts at the OS Default size.
    /// Default is true.
    /// </summary>
    /// <remarks>
    /// Overrides <see cref="PhotinoWindow.Height"/> and <see cref="PhotinoWindow.Width"/> properties.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="useOsDefault">Whether the OS Default should be used.</param>
    public PhotinoWindow SetUseOsDefaultSize(bool useOsDefault)
    {
        Log($".{nameof(SetUseOsDefaultSize)}({useOsDefault})");
        UseOsDefaultSize = useOsDefault;
        return this;
    }

    /// <summary>
    /// When true the native window starts up at the OS Default location.
    /// Default is true.
    /// </summary>
    /// <remarks>
    /// Overrides <see cref="PhotinoWindow.Left"/> (X) and <see cref="PhotinoWindow.Top"/> (Y) properties.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="useOsDefault">Whether the OS Default should be used.</param>
    public PhotinoWindow SetUseOsDefaultLocation(bool useOsDefault)
    {
        Log($".{nameof(SetUseOsDefaultLocation)}({useOsDefault})");
        UseOsDefaultLocation = useOsDefault;
        return this;
    }

    #endregion

    #region Geometry

    /// <summary>
    /// Sets the native window Size. This represents the <see cref="PhotinoWindow.Width"/> and the <see cref="PhotinoWindow.Height"/> of the window in pixels.
    /// The default Size is 0,0.
    /// </summary>
    /// <seealso cref="UseOsDefaultSize"/>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="size">Width &amp; Height</param>
    public PhotinoWindow SetSize(Size size)
    {
        Log($".{nameof(SetSize)}({size})");
        Size = size;
        return this;
    }

    /// <summary>
    /// Sets the native window Size. This represents the <see cref="PhotinoWindow.Width"/> and the <see cref="PhotinoWindow.Height"/> of the window in pixels.
    /// The default Size is 0,0.
    /// </summary>
    /// <seealso cref="UseOsDefaultSize"/>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="width">Width in pixels</param>
    /// <param name="height">Height in pixels</param>
    public PhotinoWindow SetSize(int width, int height)
    {
        Log($".{nameof(SetSize)}({width}, {height})");
        Size = new Size(width, height);
        return this;
    }

    /// <summary>
    /// Sets the native window width in pixels.
    /// Default is 0.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <seealso cref="UseOsDefaultSize"/>
    /// <param name="width">Width in pixels</param>
    public PhotinoWindow SetWidth(int width)
    {
        Log($".{nameof(SetWidth)}({width})");
        Width = width;
        return this;
    }

    /// <summary>
    /// Sets the native window <see cref="PhotinoWindow.Height"/> in pixels.
    /// Default is 0.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <seealso cref="UseOsDefaultSize"/>
    /// <param name="height">Height in pixels</param>
    public PhotinoWindow SetHeight(int height)
    {
        Log($".{nameof(SetHeight)}({height})");
        Height = height;
        return this;
    }

    /// <summary>
    /// Sets the native window <see cref="PhotinoWindow.Left"/> (X) and <see cref="PhotinoWindow.Top"/> coordinates (Y) in pixels.
    /// Default is 0,0 which means the window will be aligned to the top left edge of the screen.
    /// </summary>
    /// <seealso cref="UseOsDefaultLocation" />
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="location">Location as a <see cref="Point"/></param>
    public PhotinoWindow SetLocation(Point location)
    {
        Log($".{nameof(SetLocation)}({location})");
        Location = location;
        return this;
    }

    /// <summary>
    /// Sets the native window to a new <see cref="PhotinoWindow.Left"/> (X) coordinate in pixels.
    /// Default is 0.
    /// </summary>
    /// <seealso cref="UseOsDefaultLocation" />
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="left">Position in pixels from the left (X).</param>
    public PhotinoWindow SetLeft(int left)
    {
        Log($".{nameof(SetLeft)}({left})");
        Left = left;
        return this;
    }

    /// <summary>
    /// Sets the native window <see cref="PhotinoWindow.Top"/> (Y) coordinate in pixels.
    /// Default is 0.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <seealso cref="UseOsDefaultLocation"/>
    /// <param name="top">Position in pixels from the top (Y).</param>
    public PhotinoWindow SetTop(int top)
    {
        Log($".{nameof(SetTop)}({top})");
        Top = top;
        return this;
    }

    /// <summary>
    /// Moves the native window to the specified location on the screen in pixels using a Point.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="location">Position as <see cref="Point"/></param>
    /// <param name="allowOutsideWorkArea">Whether the window can go off-screen (work area)</param>
    public PhotinoWindow MoveTo(Point location, bool allowOutsideWorkArea = false)
    {
        Log($".{nameof(MoveTo)}({location}, {allowOutsideWorkArea})");

        if (LogVerbosity > 2)
        {
            Log($"  Current location: {Location}");
            Log($"  New location: {location}");
        }

        // If the window is outside the work area,
        // recalculate the position and continue.
        //When window isn't initialized yet, cannot determine screen size.
        if (allowOutsideWorkArea == false && _nativeInstance != IntPtr.Zero)
        {
            int horizontalWindowEdge = location.X + Width;
            int verticalWindowEdge = location.Y + Height;

            int horizontalWorkAreaEdge = MainMonitor.WorkArea.Width;
            int verticalWorkAreaEdge = MainMonitor.WorkArea.Height;

            bool isOutsideHorizontalWorkArea = horizontalWindowEdge > horizontalWorkAreaEdge;
            bool isOutsideVerticalWorkArea = verticalWindowEdge > verticalWorkAreaEdge;

            var locationInsideWorkArea = new Point(
                isOutsideHorizontalWorkArea ? horizontalWorkAreaEdge - Width : location.X,
                isOutsideVerticalWorkArea ? verticalWorkAreaEdge - Height : location.Y
            );

            location = locationInsideWorkArea;
        }

        // Bug:
        // For some reason the vertical position is not handled correctly.
        // Whenever a positive value is set, the window appears at the
        // very bottom of the screen and the only visible thing is the
        // application window title bar. As a workaround we make a
        // negative value out of the vertical position to "pull" the window up.
        // Note:
        // This behavior seems to be a macOS thing. In the Photino.Native
        // project files it is commented to be expected behavior for macOS.
        // There is some code trying to mitigate this problem, but it might
        // not work as expected. Further investigation is necessary.
        // Update:
        // This behavior seems to have changed with macOS Sonoma.
        // Therefore, we determine the version of macOS and only apply the
        // workaround for older versions.
        if (_nativeInstance != IntPtr.Zero && Platform.IsMacOS && Platform.MacOS.IsPreSonoma)
        {
            var workArea = MainMonitor.WorkArea.Size;
            location.Y = location.Y >= 0
                ? location.Y - workArea.Height
                : location.Y;
        }

        Location = location;

        return this;
    }

    /// <summary>
    /// Moves the native window to the specified location on the screen in pixels
    /// using <see cref="PhotinoWindow.Left"/> (X) and <see cref="PhotinoWindow.Top"/> (Y) properties.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="left">Position from left in pixels</param>
    /// <param name="top">Position from top in pixels</param>
    /// <param name="allowOutsideWorkArea">Whether the window can go off-screen (work area)</param>
    public PhotinoWindow MoveTo(int left, int top, bool allowOutsideWorkArea = false)
    {
        Log($".{nameof(MoveTo)}({left}, {top}, {allowOutsideWorkArea})");
        return MoveTo(new Point(left, top), allowOutsideWorkArea);
    }

    /// <summary>
    /// Moves the native window relative to its current location on the screen
    /// using a <see cref="Point"/>.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="offset">Relative offset</param>
    public PhotinoWindow Offset(Point offset)
    {
        Log($".{nameof(Offset)}({offset})");
        var location = Location;
        int left = location.X + offset.X;
        int top = location.Y + offset.Y;
        return MoveTo(left, top);
    }

    /// <summary>
    /// Moves the native window relative to its current location on the screen in pixels
    /// using <see cref="PhotinoWindow.Left"/> (X) and <see cref="PhotinoWindow.Top"/> (Y) properties.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="left">Relative offset from left in pixels</param>
    /// <param name="top">Relative offset from top in pixels</param>
    public PhotinoWindow Offset(int left, int top)
    {
        Log($".{nameof(Offset)}({left}, {top})");
        return Offset(new Point(left, top));
    }

    /// <summary>
    /// Sets the native window minimum size in pixels.
    /// </summary>
    /// <param name="minWidth">
    /// The minimum window width in pixels.
    /// </param>
    /// <param name="minHeight">
    /// The minimum window height in pixels.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMinSize(int minWidth, int minHeight)
    {
        Log($".{nameof(SetMinSize)}({minWidth}, {minHeight})");
        MinSize = new Point(minWidth, minHeight);
        return this;
    }

    /// <summary>
    /// Sets the native window minimum width in pixels.
    /// </summary>
    /// <param name="minWidth">
    /// The minimum window width in pixels.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMinWidth(int minWidth)
    {
        Log($".{nameof(SetMinWidth)}({minWidth})");
        MinWidth = minWidth;
        return this;
    }

    /// <summary>
    /// Sets the native window minimum height in pixels.
    /// </summary>
    /// <param name="minHeight">
    /// The minimum window height in pixels.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMinHeight(int minHeight)
    {
        Log($".{nameof(SetMinHeight)}({minHeight})");
        MinHeight = minHeight;
        return this;
    }

    /// <summary>
    /// Sets the native window maximum size in pixels.
    /// </summary>
    /// <param name="maxWidth">
    /// The maximum window width in pixels.
    /// </param>
    /// <param name="maxHeight">
    /// The maximum window height in pixels.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMaxSize(int maxWidth, int maxHeight)
    {
        Log($".{nameof(SetMaxSize)}({maxWidth}, {maxHeight})");
        MaxSize = new Point(maxWidth, maxHeight);
        return this;
    }

    /// <summary>
    /// Sets the native window maximum width in pixels.
    /// </summary>
    /// <param name="maxWidth">
    /// The maximum window width in pixels.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMaxWidth(int maxWidth)
    {
        Log($".{nameof(SetMaxWidth)}({maxWidth})");
        MaxWidth = maxWidth;
        return this;
    }

    /// <summary>
    /// Sets the native window maximum height in pixels.
    /// </summary>
    /// <param name="maxHeight">
    /// The maximum window height in pixels.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMaxHeight(int maxHeight)
    {
        Log($".{nameof(SetMaxHeight)}({maxHeight})");
        MaxHeight = maxHeight;
        return this;
    }

    /// <summary>
    /// Centers the native window on the primary display.
    /// </summary>
    /// <remarks>
    /// If called before native window initialization, the window will be centered on startup.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow Center()
    {
        Log($".{nameof(Center)}()");
        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
            _startupParameters.CenterOnInitialize = true;
        else
            Invoke(() => Photino_Center(_nativeInstance));

        return this;
    }

    #endregion

    #region Window state

    /// <summary>
    /// Sets whether the native window should be fullscreen.
    /// </summary>
    /// <remarks>
    /// If called before native window initialization, the window will be fullscreen on startup.
    /// Fullscreen is incompatible with minimized and maximized startup states.
    /// </remarks>
    /// <param name="fullScreen">
    /// <see langword="true"/> to enter fullscreen mode; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    public PhotinoWindow SetFullScreen(bool fullScreen)
    {
        Log($".{nameof(SetFullScreen)}({fullScreen})");
        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
        {
            _startupParameters.FullScreen = fullScreen;
            if (fullScreen)
            {
                _startupParameters.Maximized = false;
                _startupParameters.Minimized = false;
            }
        }
        else
        {
            Invoke(() => Photino_SetFullScreen(_nativeInstance, (byte)(fullScreen ? 1 : 0)));
        }

        return this;
    }

    /// <summary>
    /// Sets whether the native window is maximized.
    /// Default is false.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="maximized">Whether the window should be maximized.</param>
    public PhotinoWindow SetMaximized(bool maximized)
    {
        Log($".{nameof(SetMaximized)}({maximized})");
        ThrowIfClosed();

        if (maximized)
            return Maximize();

        if (_nativeInstance == IntPtr.Zero)
            _startupParameters.Maximized = false;
        else
            Invoke(() => Photino_SetMaximized(_nativeInstance, 0));

        return this;
    }

    /// <summary>
    /// Sets whether the native window is minimized (hidden).
    /// Default is false.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="minimized">Whether the window should be minimized.</param>
    public PhotinoWindow SetMinimized(bool minimized)
    {
        Log($".{nameof(SetMinimized)}({minimized})");
        ThrowIfClosed();

        if (minimized)
            return Minimize();

        if (_nativeInstance == IntPtr.Zero)
            _startupParameters.Minimized = false;
        else
            Invoke(() => Photino_SetMinimized(_nativeInstance, 0));

        return this;
    }

    /// <summary>
    /// Sets whether the native window can be resized by the user.
    /// Default is true.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="resizable">Whether the window is resizable</param>
    public PhotinoWindow SetResizable(bool resizable)
    {
        Log($".{nameof(SetResizable)}({resizable})");
        Resizable = resizable;
        return this;
    }

    /// <summary>
    /// Sets whether the native window is always at the top of the z-order.
    /// Default is false.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="topmost">Whether the window is at the top</param>
    public PhotinoWindow SetTopmost(bool topmost)
    {
        Log($".{nameof(SetTopmost)}({topmost})");
        Topmost = topmost;
        return this;
    }

    #endregion

    #region Chrome / appearance

    /// <summary>
    /// When true, the native window will appear without a title bar or border.
    /// By default, this is set to false.
    /// </summary>
    /// <remarks>
    /// The user has to supply titlebar, border, dragging and resizing manually.
    /// Use <see cref="BeginWindowDrag()"/> and <see cref="BeginWindowResize(PhotinoWindowEdge)"/>
    /// to drive dragging and resizing from a custom title bar and border.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="chromeless">Whether the window should be chromeless</param>
    public PhotinoWindow SetChromeless(bool chromeless)
    {
        Log($".{nameof(SetChromeless)}({chromeless})");
        Chromeless = chromeless;
        return this;
    }

    /// <summary>
    /// Enables or disables native window transparency.
    /// </summary>
    /// <remarks>
    /// Transparency requires a chromeless window and page content with an alpha-based background.
    /// </remarks>
    /// <param name="enabled">
    /// <see langword="true"/> to enable transparency; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetTransparent(bool enabled)
    {
        Log($".{nameof(SetTransparent)}({enabled})");
        Transparent = enabled;
        return this;
    }

    /// <summary>
    /// Starts an OS-level drag of the window from the current mouse position, as if
    /// the user had pressed on a native title bar. Call this from a pointer-down
    /// handler on a custom title bar to make a chromeless window draggable.
    /// </summary>
    /// <remarks>
    /// The mouse button must still be pressed when this is called; the drag follows
    /// the cursor until the button is released. Currently implemented on Windows;
    /// on Linux and macOS this is a no-op pending platform support.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <seealso cref="BeginWindowResize(PhotinoWindowEdge)" />
    /// <seealso cref="SetChromeless(bool)" />
    public PhotinoWindow BeginWindowDrag()
    {
        Log($".{nameof(BeginWindowDrag)}()");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_BeginWindowDrag(_nativeInstance));
        return this;
    }

    /// <summary>
    /// Starts an OS-level resize of the window from the given edge or corner, as if
    /// the user had dragged that part of a native window border. Call this from a
    /// pointer-down handler on a custom resize grip to make a chromeless window
    /// resizable.
    /// </summary>
    /// <remarks>
    /// The mouse button must still be pressed when this is called; the resize follows
    /// the cursor until the button is released. Currently implemented on Windows;
    /// on Linux and macOS this is a no-op pending platform support.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="edge">The edge or corner to resize from.</param>
    /// <seealso cref="BeginWindowDrag()" />
    /// <seealso cref="SetChromeless(bool)" />
    public PhotinoWindow BeginWindowResize(PhotinoWindowEdge edge)
    {
        Log($".{nameof(BeginWindowResize)}({edge})");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_BeginWindowResize(_nativeInstance, edge));
        return this;
    }

    #endregion

    #region Browser content

    /// <summary>
    /// Loads the specified <see cref="Uri"/> into the browser control.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// If called before native window initialization, the URI is stored as startup content.
    /// Otherwise, the current browser content is navigated immediately.
    /// Runtime navigation requires an absolute URI.
    /// </remarks>
    /// <param name="uri">
    /// The URI to load. Relative URIs are allowed before native window initialization.
    /// Runtime navigation requires an absolute URI.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="uri"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the window is already initialized and <paramref name="uri"/> is not an absolute URI.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed.
    /// </exception>
    public PhotinoWindow Load(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        Log($".{nameof(Load)}({uri})");

        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
        {
            _startupParameters.StartUrl = uri.ToString();
            _startupParameters.StartString = null;
        }
        else
        {
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("Runtime navigation URI must be absolute.", nameof(uri));

            Invoke(() => Photino_NavigateToUrl(_nativeInstance, uri.ToString()));
        }

        return this;
    }

    /// <summary>
    /// Loads the specified path, HTTP/HTTPS URL, file URI, or registered custom-scheme URI into the browser control.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// If called before native window initialization, the resolved content is used as startup content.
    /// Otherwise, the current browser content is navigated immediately.
    /// Relative paths are resolved first against the current working directory and then against
    /// <see cref="AppContext.BaseDirectory"/>.
    /// Registered custom-scheme URI strings, such as <c>app://index.html</c>, are loaded as URIs.
    /// </remarks>
    /// <param name="path">
    /// A local file path, relative file path, HTTP/HTTPS URL, file URI, or registered custom-scheme URI to load.
    /// </param>
    public PhotinoWindow Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        Log($".{nameof(Load)}({path})");
        ThrowIfClosed();

        // ––––––––––––––––––––––
        // SECURITY RISK!
        // This needs validation!
        // ––––––––––––––––––––––
        // Open a scheme string path
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp ||
             uri.Scheme == Uri.UriSchemeHttps ||
             uri.Scheme == Uri.UriSchemeFile ||
             IsCustomSchemeRegistered(uri.Scheme)))
        {
            return Load(uri);
        }

        // Open a file resource string path
        string absolutePath = Path.GetFullPath(path);

        // For bundled app it can be necessary to consider
        // the app context base directory. Check there too.
        if (File.Exists(absolutePath) == false)
        {
            absolutePath = Path.Combine(AppContext.BaseDirectory, path);

            if (File.Exists(absolutePath) == false)
            {
                Log($" ** File \"{path}\" could not be found.");
                return this;
            }
        }

        return Load(new Uri(absolutePath, UriKind.Absolute));
    }

    /// <summary>
    /// Loads a raw string into the browser control.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// If called before native window initialization, the content is loaded on startup.
    /// Otherwise, the current browser content is navigated immediately.
    /// </remarks>
    /// <param name="content">Raw content (such as HTML)</param>
    public PhotinoWindow LoadString(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var shortContent = content.Length > 50 ? string.Concat(content.AsSpan(0, 50), "...") : content;
        Log($".{nameof(LoadString)}({shortContent})");
        ThrowIfClosed();

        if (_nativeInstance == IntPtr.Zero)
        {
            _startupParameters.StartString = content;
            _startupParameters.StartUrl = null;
        }
        else
            Invoke(() => Photino_NavigateToString(_nativeInstance, content));

        return this;
    }

    #endregion

    #region Browser behavior

    /// <summary>
    /// When true, the user can access the browser control's context menu.
    /// By default, this is set to true.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="enabled">Whether the context menu should be available</param>
    public PhotinoWindow SetContextMenuEnabled(bool enabled)
    {
        Log($".{nameof(SetContextMenuEnabled)}({enabled})");
        ContextMenuEnabled = enabled;
        return this;
    }

    /// <summary>
    /// When true, the user can zoom.
    /// By default, this is set to true.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="enabled">Whether the zoom should be available</param>
    public PhotinoWindow SetZoomEnabled(bool enabled)
    {
        Log($".{nameof(SetZoomEnabled)}({enabled})");
        ZoomEnabled = enabled;
        return this;
    }

    /// <summary>
    /// When true, the user can access the browser control's developer tools.
    /// By default, this is set to true.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="enabled">Whether developer tools should be available</param>
    public PhotinoWindow SetDevToolsEnabled(bool enabled)
    {
        Log($".{nameof(SetDevToolsEnabled)}({enabled})");
        DevToolsEnabled = enabled;
        return this;
    }

    /// <summary>
    /// Sets the native browser control <see cref="PhotinoWindow.Zoom"/>.
    /// Default is 100.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="zoom">Zoom level (in percent).</param>
    /// <example>100 = 100%, 50 = 50%</example>
    public PhotinoWindow SetZoom(int zoom)
    {
        Log($".{nameof(SetZoom)}({zoom})");
        Zoom = zoom;
        return this;
    }

    /// <summary>
    /// Sets the browser control user agent at initialization.
    /// </summary>
    /// <param name="userAgent">
    /// The user agent string.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetUserAgent(string userAgent)
    {
        Log($".{nameof(SetUserAgent)}({userAgent})");
        UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.BrowserControlInitParameters"/> platform‑specific
    /// initialization parameters for the native browser control on startup.
    /// Default is none.
    /// </summary>
    /// <remarks>
    /// The value is passed to the native browser backend during initialization.
    /// Supported format and options are platform-specific.
    /// <para><b>Windows:</b> WebView2-specific arguments (space-separated).</para>
    /// <para>See:</para>
    /// <para>https://peter.sh/experiments/chromium-command-line-switches/</para>
    /// <para>https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2environmentoptions.additionalbrowserarguments</para>
    /// <para>https://www.chromium.org/developers/how-tos/run-chromium-with-flags/</para>
    ///
    /// <para><b>Linux:</b> WebKit2GTK-specific JSON settings.</para>
    /// <para>Example: <c>{ "set_enable_encrypted_media": true }</c></para>
    /// <para>See:</para>
    /// <para>https://webkitgtk.org/reference/webkit2gtk/2.5.1/WebKitSettings.html</para>
    /// <para>https://lazka.github.io/pgi-docs/WebKit2-4.0/classes/Settings.html</para>
    ///
    /// <para><b>macOS:</b> WebKit (WKWebView) JSON settings.</para>
    /// <para>Example: <c>{ "minimumFontSize": 8 }</c></para>
    /// <para>See:</para>
    /// <para>https://developer.apple.com/documentation/webkit/wkwebviewconfiguration</para>
    /// <para>https://developer.apple.com/documentation/webkit/wkpreferences</para>
    /// </remarks>
    /// <param name="parameters">Platform‑specific initialization string.</param>
    /// <returns>The current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetBrowserControlInitParameters(string parameters)
    {
        Log($".{nameof(SetBrowserControlInitParameters)}({parameters})");
        BrowserControlInitParameters = parameters;
        return this;
    }

    #endregion

    #region Browser permissions / security

    /// <summary>
    /// Sets whether browser permission requests are granted automatically.
    /// </summary>
    /// <param name="grant">
    /// <see langword="true"/> to grant browser permission requests automatically; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetGrantBrowserPermissions(bool grant)
    {
        Log($".{nameof(SetGrantBrowserPermissions)}({grant})");
        GrantBrowserPermissions = grant;
        return this;
    }

    /// <summary>
    /// Enables or disables browser media autoplay at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable media autoplay; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMediaAutoplayEnabled(bool enable)
    {
        Log($".{nameof(SetMediaAutoplayEnabled)}({enable})");
        MediaAutoplayEnabled = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables browser file system access at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable file system access; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetFileSystemAccessEnabled(bool enable)
    {
        Log($".{nameof(SetFileSystemAccessEnabled)}({enable})");
        FileSystemAccessEnabled = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables browser web security at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable web security; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetWebSecurityEnabled(bool enable)
    {
        Log($".{nameof(SetWebSecurityEnabled)}({enable})");
        WebSecurityEnabled = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables JavaScript clipboard access at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable JavaScript clipboard access; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetJavascriptClipboardAccessEnabled(bool enable)
    {
        Log($".{nameof(SetJavascriptClipboardAccessEnabled)}({enable})");
        JavascriptClipboardAccessEnabled = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables browser media stream support at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable media stream support; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetMediaStreamEnabled(bool enable)
    {
        Log($".{nameof(SetMediaStreamEnabled)}({enable})");
        MediaStreamEnabled = enable;
        return this;
    }

    /// <summary>
    /// Enables or disables ignoring browser certificate errors at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to ignore certificate errors; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetIgnoreCertificateErrorsEnabled(bool enable)
    {
        Log($".{nameof(SetIgnoreCertificateErrorsEnabled)}({enable})");
        IgnoreCertificateErrorsEnabled = enable;
        return this;
    }

    #endregion

    #region Browser platform/features

    /// <summary>
    /// Enables or disables browser smooth scrolling at initialization.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable smooth scrolling; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetSmoothScrollingEnabled(bool enable)
    {
        Log($".{nameof(SetSmoothScrollingEnabled)}({enable})");
        SmoothScrollingEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets the local path to store temp files for browser control.
    /// Default is the user's AppDataLocal folder.
    /// </summary>
    /// <remarks>
    /// Only available on Windows.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="tempFilesPath">Path to temp file's directory.</param>
    public PhotinoWindow SetTemporaryFilesPath(string? tempFilesPath)
    {
        Log($".{nameof(SetTemporaryFilesPath)}({tempFilesPath})");
        TemporaryFilesPath = tempFilesPath;
        return this;
    }

    /// <summary>
    /// Set runtime path for WebView2 so that developers can use Photino on Windows using the "Fixed Version" deployment module of the WebView2 runtime.
    /// </summary>
    /// <remarks>
    /// This only works on Windows.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <seealso href="https://docs.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution" />
    /// <param name="data">Runtime path for WebView2, or <see langword="null"/> to clear it.</param>
    public PhotinoWindow Win32SetWebView2Path(string? data)
    {
        Log($".{nameof(Win32SetWebView2Path)}({data})");

        if (Platform.IsWindows)
            Photino_setWebView2RuntimePath_win32(data);
        else
            Log($"{nameof(Win32SetWebView2Path)} is only supported on the Windows platform");

        return this;
    }

    /// <summary>
    /// Clears the autofill data in the browser control.
    /// </summary>
    /// <remarks>
    /// This method is only supported on the Windows platform.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow ClearBrowserAutoFill()
    {
        Log($".{nameof(ClearBrowserAutoFill)}()");
        ThrowIfClosedOrNotInitialized();

        if (Platform.IsWindows)
            Invoke(() => Photino_ClearBrowserAutoFill(_nativeInstance));
        else
            Log($"{nameof(ClearBrowserAutoFill)} is only supported on the Windows platform");

        return this;
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Enables or disables native notifications.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to enable notifications; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetNotificationsEnabled(bool enable)
    {
        Log($".{nameof(SetNotificationsEnabled)}({enable})");
        NotificationsEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets the registration id for toast notifications.
    /// </summary>
    /// <remarks>
    /// This only works on Windows. If not specified, the window title is used.
    /// </remarks>
    /// <param name="notificationRegistrationId">
    /// The notification registration id.
    /// </param>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow SetNotificationRegistrationId(string notificationRegistrationId)
    {
        Log($".{nameof(SetNotificationRegistrationId)}({notificationRegistrationId})");
        NotificationRegistrationId = notificationRegistrationId;
        return this;
    }

    #endregion

    #region Diagnostics

    /// <summary>
    /// Sets the logging verbosity to standard output (Console/Terminal).
    /// 0 = Critical Only
    /// 1 = Critical and Warning
    /// 2 = Verbose
    /// >2 = All Details
    /// Default is 2.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="verbosity">Verbosity as integer</param>
    public PhotinoWindow SetLogVerbosity(int verbosity)
    {
        Log($".{nameof(SetLogVerbosity)}({verbosity})");
        LogVerbosity = verbosity;
        return this;
    }

    #endregion
}
