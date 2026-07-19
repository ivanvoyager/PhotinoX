using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Photino.NET.Utils;

namespace Photino.NET;

using static NativeMethods;

/// <summary>
/// The PhotinoWindow class represents a window in a Photino-based desktop application.
/// </summary>
public partial class PhotinoWindow
{
    /// <summary>
    /// Parameters sent to Photino.Native to start a new instance of a Photino.Native window.
    /// </summary>

    private PhotinoNativeParameters _startupParameters = new()
    {
        Resizable = true,   //These values can't be initialized within the struct itself. Set required defaults.
        ContextMenuEnabled = true,
        ZoomEnabled = true,
        CustomSchemeNames = new string[MaxCustomSchemeNames],
        DevToolsEnabled = true,
        GrantBrowserPermissions = true,
        UserAgent = "PhotinoX WebView",
        MediaAutoplayEnabled = true,
        FileSystemAccessEnabled = true,
        WebSecurityEnabled = true,
        JavascriptClipboardAccessEnabled = true,
        MediaStreamEnabled = true,
        SmoothScrollingEnabled = true,
        IgnoreCertificateErrorsEnabled = false,
        NotificationsEnabled = true,
        TemporaryFilesPath = Platform.IsWindows
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Photino")
            : null,
        Title = DefaultTitle,
        UseOsDefaultLocation = true,
        UseOsDefaultSize = true,
        Zoom = 100,
        MaxHeight = int.MaxValue,
        MaxWidth = int.MaxValue,
    };

    private const string DefaultTitle = "PhotinoX";

    private IntPtr _nativeInstance;
    private bool _suppressClosing;

    private string? _title = DefaultTitle;

    /// <summary>
    /// Gets a value indicating whether the native window has been initialized and has not been closed.
    /// </summary>
    public bool IsInitialized => _nativeInstance != IntPtr.Zero;

    /// <summary>
    /// Gets a value indicating whether the window has already been closed.
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Gets the platform-specific native window reference.
    /// </summary>
    /// <remarks>
    /// On Windows, returns an HWND.
    /// On Linux, returns a GtkWidget* whose runtime type is GtkWindow.
    /// On macOS, returns an NSWindow*.
    /// The returned pointer is owned by Photino and must not be destroyed, released, or unreferenced by the caller.
    /// Platform-specific APIs using this pointer must follow the platform UI-thread rules.
    /// </remarks>
    /// <value>
    /// The platform-specific native window reference as an <see cref="IntPtr"/>.
    /// </value>
    /// <exception cref="InvalidOperationException">Thrown when the window is not initialized or has already been closed.</exception>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    public IntPtr WindowHandle
    {
        get
        {
            ThrowIfClosedOrNotInitialized();

            var handle = IntPtr.Zero;
            if (Platform.IsWindows)
            {
                Invoke(() => handle = Photino_getHwnd_win32(_nativeInstance));
                return handle;
            }

            if (Platform.IsLinux)
            {
                Invoke(() => handle = Photino_getGtkWidget_linux(_nativeInstance));
                return handle;
            }

            if (Platform.IsMacOS)
            {
                Invoke(() => handle = Photino_getNSWindow_mac(_nativeInstance));
                return handle;
            }

            throw new PlatformNotSupportedException($"{nameof(WindowHandle)} not supported on current platform.");
        }
    }

    /// <summary>
    /// Gets list of information for each monitor from the native window.
    /// This property represents a list of Monitor objects associated to each display monitor.
    /// </summary>
    /// <remarks>
    /// If called when the native instance of the window is not initialized, it will throw an <see cref="InvalidOperationException"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the window is not initialized or has already been closed.</exception>
    /// <returns>
    /// A read-only list of Monitor objects representing information about each display monitor.
    /// </returns>
    public IReadOnlyList<Monitor> Monitors
    {
        get
        {
            ThrowIfClosedOrNotInitialized();

            var monitors = new List<Monitor>();

            Invoke(() => Photino_GetAllMonitors(_nativeInstance, Callback));

            return monitors;

            int Callback(in NativeMonitor monitor)
            {
                monitors.Add(new Monitor(monitor));
                return 1;
            }
        }
    }

    /// <summary>
    /// Retrieves the primary monitor information from the native window instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the window is not initialized or has already been closed.</exception>
    /// <returns>
    /// Returns a Monitor object representing the main monitor. The main monitor is the first monitor in the list of available monitors.
    /// </returns>
    public Monitor MainMonitor
    {
        get
        {
            ThrowIfClosedOrNotInitialized();

            return Monitors[0];
        }
    }

    /// <summary>
    /// Gets the dots per inch (DPI) for the primary display from the native window.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    public uint ScreenDpi
    {
        get
        {
            ThrowIfClosedOrNotInitialized();

            uint dpi = 0;
            Invoke(() => dpi = Photino_GetScreenDpi(_nativeInstance));
            return dpi;
        }
    }

    /// <summary>
    /// Gets a unique GUID to identify the native window.
    /// </summary>
    /// <remarks>
    /// This property is not currently utilized by the Photino framework.
    /// </remarks>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// When true, the native window will appear centered on the screen. By default, this is set to false.
    /// </summary>
    public bool Centered
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.CenterOnInitialize;
            return false;
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.CenterOnInitialize = value;
            else
                Invoke(() => Photino_Center(_nativeInstance));
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the native window should be chromeless.
    /// When true, the native window will appear without a title bar or border.
    /// By default, this is set to false.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    /// <remarks>
    /// The user has to supply titlebar, border, dragging and resizing manually.
    /// </remarks>
    public bool Chromeless
    {
        get
        {
            return _startupParameters.Chromeless;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            _startupParameters.Chromeless = value;
        }
    }

    /// <summary>
    /// When true, the native window and browser control can be displayed with transparent background.
    /// Html document's body background must have alpha-based value.
    /// WebView2 on Windows can only be fully transparent or fully opaque.
    /// By default, this is set to false.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// On Windows, thrown if trying to set value after native window is initialized.
    /// </exception>
    public bool Transparent
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Transparent;

            byte enabled = 0;
            Invoke(() => Photino_GetTransparentEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosed();

            if (Transparent != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.Transparent = value;
                else
                {
                    if (Platform.IsWindows)
                        throw new InvalidOperationException("Transparent can only be set on Windows before the native window is instantiated.");
                    else
                    {
                        Log($"Invoking Photino_SetTransparentEnabled({value})");
                        Invoke(() => Photino_SetTransparentEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
                    }
                }
            }
        }
    }

    /// <summary>
    /// When true, the user can access the browser control's context menu.
    /// By default, this is set to true.
    /// </summary>
    public bool ContextMenuEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.ContextMenuEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetContextMenuEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosed();

            if (ContextMenuEnabled != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.ContextMenuEnabled = value;
                else
                    Invoke(() => Photino_SetContextMenuEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    /// <summary>
    /// When true, the user can zoom.
    /// By default, this is set to true.
    /// </summary>
    public bool ZoomEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.ZoomEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetZoomEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosed();

            if (ZoomEnabled != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.ZoomEnabled = value;
                else
                    Invoke(() => Photino_SetZoomEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    /// <summary>
    /// When true, the user can access the browser control's developer tools.
    /// By default, this is set to true.
    /// </summary>
    public bool DevToolsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.DevToolsEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetDevToolsEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosed();

            if (DevToolsEnabled != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.DevToolsEnabled = value;
                else
                    Invoke(() => Photino_SetDevToolsEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    public bool MediaAutoplayEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.MediaAutoplayEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetMediaAutoplayEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (MediaAutoplayEnabled != value)
            {
                _startupParameters.MediaAutoplayEnabled = value;
            }
        }
    }

    public string? UserAgent
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.UserAgent;

            string? userAgent = null;
            Invoke(() =>
            {
                var ptr = Photino_GetUserAgent(_nativeInstance);
                try
                {
                    userAgent = ptr != IntPtr.Zero
                        ? Marshal.PtrToStringUTF8(ptr)
                        : null;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Photino_FreeString(ptr);
                }
            });
            return userAgent;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (UserAgent != value)
            {
                _startupParameters.UserAgent = value;
            }
        }
    }

    public bool FileSystemAccessEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.FileSystemAccessEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetFileSystemAccessEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (FileSystemAccessEnabled != value)
            {
                _startupParameters.FileSystemAccessEnabled = value;
            }
        }
    }

    public bool WebSecurityEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.WebSecurityEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetWebSecurityEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (WebSecurityEnabled != value)
            {
                _startupParameters.WebSecurityEnabled = value;
            }
        }
    }

    public bool JavascriptClipboardAccessEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.JavascriptClipboardAccessEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetJavascriptClipboardAccessEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (JavascriptClipboardAccessEnabled != value)
            {
                _startupParameters.JavascriptClipboardAccessEnabled = value;
            }
        }
    }

    public bool MediaStreamEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.MediaStreamEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetMediaStreamEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (MediaStreamEnabled != value)
            {
                _startupParameters.MediaStreamEnabled = value;
            }
        }
    }

    public bool SmoothScrollingEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.SmoothScrollingEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetSmoothScrollingEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (SmoothScrollingEnabled != value)
            {
                _startupParameters.SmoothScrollingEnabled = value;
            }
        }
    }

    public bool IgnoreCertificateErrorsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.IgnoreCertificateErrorsEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetIgnoreCertificateErrorsEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (IgnoreCertificateErrorsEnabled != value)
            {
                _startupParameters.IgnoreCertificateErrorsEnabled = value;
            }
        }
    }

    public bool NotificationsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.NotificationsEnabled;

            byte enabled = 0;
            Invoke(() => Photino_GetNotificationsEnabled(_nativeInstance, out enabled));
            return enabled != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (NotificationsEnabled != value)
            {
                _startupParameters.NotificationsEnabled = value;
            }
        }
    }


    /// <summary>
    /// This property returns or sets the fullscreen status of the window.
    /// When set to true, the native window will cover the entire screen, similar to kiosk mode.
    /// By default, this is set to false.
    /// </summary>
    public bool FullScreen
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.FullScreen;

            byte fullScreen = 0;
            Invoke(() => Photino_GetFullScreen(_nativeInstance, out fullScreen));
            return fullScreen != 0;
        }
        set
        {
            ThrowIfClosed();

            if (FullScreen != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.FullScreen = value;
                else
                    Invoke(() => Photino_SetFullScreen(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    ///<summary>
    /// Gets or Sets whether the native browser control grants all requests for access to local resources
    /// such as the users camera and microphone. By default, this is set to true.
    /// </summary>
    /// <remarks>
    /// This only works on Windows.
    /// </remarks>
    public bool GrantBrowserPermissions
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.GrantBrowserPermissions;

            byte grant = 0;
            Invoke(() => Photino_GetGrantBrowserPermissions(_nativeInstance, out grant));
            return grant != 0;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            if (GrantBrowserPermissions != value)
            {
                _startupParameters.GrantBrowserPermissions = value;
            }
        }
    }

    /// /// <summary>
    /// Gets or Sets the Height property of the native window in pixels. 
    /// Default value is 0.
    /// </summary>
    /// <seealso cref="UseOsDefaultSize" />
    public int Height
    {
        get => Size.Height;
        set
        {
            ThrowIfClosed();

            var currentSize = Size;
            if (currentSize.Height != value)
                Size = currentSize with { Height = value };
        }
    }

    /// <summary>
    /// Gets or sets the icon file for the native window title bar.
    /// The file must be located on the local machine and cannot be a URL. The default is none.
    /// </summary>
    /// <remarks>
    /// This only works on Windows and Linux.
    /// </remarks>
    /// <value>
    /// The file path to the icon.
    /// </value>
    /// <exception cref="System.ArgumentException">Icon file: {value} does not exist.</exception>
    public string? IconFile
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.WindowIconFile;

            string? iconFile = null;
            Invoke(() =>
            {
                var ptr = Photino_GetIconFile(_nativeInstance);
                try
                {
                    iconFile = ptr != IntPtr.Zero
                        ? Marshal.PtrToStringUTF8(ptr)
                        : null;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Photino_FreeString(ptr);
                }
            });
            return iconFile;
        }
        set
        {
            ThrowIfClosed();

            if (IconFile != value)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value);

                if (!File.Exists(value))
                {
                    var absolutePath = $"{AppContext.BaseDirectory}{value}";
                    if (!File.Exists(absolutePath))
                        throw new ArgumentException($"Icon file: {value} does not exist.");
                }

                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.WindowIconFile = value;
                else
                    Invoke(() => Photino_SetIconFile(_nativeInstance, value));
            }
        }
    }

    /// <summary>
    /// Gets or sets the native window Left (X) and Top coordinates (Y) in pixels.
    /// Default is 0,0 which means the window will be aligned to the top left edge of the screen.
    /// </summary>
    /// <seealso cref="UseOsDefaultLocation" />
    public Point Location
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return new Point(_startupParameters.Left, _startupParameters.Top);

            var left = 0;
            var top = 0;
            Invoke(() => Photino_GetPosition(_nativeInstance, out left, out top));
            return new Point(left, top);
        }
        set
        {
            ThrowIfClosed();

            if (Location.X != value.X || Location.Y != value.Y)
            {
                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.Left = value.X;
                    _startupParameters.Top = value.Y;
                }
                else
                    Invoke(() => Photino_SetPosition(_nativeInstance, value.X, value.Y));
            }
        }
    }

    /// <summary>
    /// Gets or sets the native window Left (X) coordinate in pixels.
    /// This represents the horizontal position of the window relative to the screen.
    /// Default value is 0 which means the window will be aligned to the left edge of the screen.
    /// </summary>
    /// <seealso cref="UseOsDefaultLocation" />
    public int Left
    {
        get => Location.X;
        set
        {
            ThrowIfClosed();

            if (Location.X != value)
                Location = Location with { X = value };
        }
    }

    /// <summary>
    /// Gets or sets whether the native window is maximized.
    /// Default is false.
    /// </summary>
    public bool Maximized
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Maximized;

            byte maximized = 0;
            Invoke(() => Photino_GetMaximized(_nativeInstance, out maximized));
            return maximized != 0;
        }
        set
        {
            ThrowIfClosed();

            if (Maximized != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.Maximized = value;
                else
                    Invoke(() => Photino_SetMaximized(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    ///<summary>Gets or set the maximum size of the native window in pixels.</summary>
    public Point MaxSize
    {
        get => new(MaxWidth, MaxHeight);
        set
        {
            ThrowIfClosed();

            if (MaxWidth != value.X || MaxHeight != value.Y)
            {
                _maxWidth = value.X;
                _maxHeight = value.Y;

                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.MaxWidth = value.X;
                    _startupParameters.MaxHeight = value.Y;
                }
                else
                    Invoke(() => Photino_SetMaxSize(_nativeInstance, value.X, value.Y));
            }
        }
    }

    ///<summary>Gets or sets the native window maximum height in pixels.</summary>
    private int _maxHeight = int.MaxValue;
    public int MaxHeight
    {
        get => _maxHeight;
        set
        {
            ThrowIfClosed();

            if (_maxHeight != value)
            {
                MaxSize = MaxSize with { Y = value };
                _maxHeight = value;
            }
        }
    }

    ///<summary>Gets or sets the native window maximum width in pixels.</summary>
    private int _maxWidth = int.MaxValue;
    public int MaxWidth
    {
        get => _maxWidth;
        set
        {
            ThrowIfClosed();

            if (_maxWidth != value)
            {
                MaxSize = MaxSize with { X = value };
                _maxWidth = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the native window is minimized (hidden).
    /// Default is false.
    /// </summary>
    public bool Minimized
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Minimized;

            byte minimized = 0;
            Invoke(() => Photino_GetMinimized(_nativeInstance, out minimized));
            return minimized != 0;
        }
        set
        {
            ThrowIfClosed();

            if (Minimized != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.Minimized = value;
                else
                    Invoke(() => Photino_SetMinimized(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    ///<summary>Gets or set the minimum size of the native window in pixels.</summary>
    public Point MinSize
    {
        get => new(MinWidth, MinHeight);
        set
        {
            ThrowIfClosed();

            if (MinWidth != value.X || MinHeight != value.Y)
            {
                _minWidth = value.X;
                _minHeight = value.Y;

                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.MinWidth = value.X;
                    _startupParameters.MinHeight = value.Y;
                }
                else
                    Invoke(() => Photino_SetMinSize(_nativeInstance, value.X, value.Y));
            }
        }
    }

    ///<summary>Gets or sets the native window minimum height in pixels.</summary>
    private int _minHeight = 0;
    public int MinHeight
    {
        get => _minHeight;
        set
        {
            ThrowIfClosed();

            if (_minHeight != value)
            {
                MinSize = MinSize with { Y = value };
                _minHeight = value;
            }
        }
    }

    ///<summary>Gets or sets the native window minimum width in pixels.</summary>
    private int _minWidth = 0;
    public int MinWidth
    {
        get => _minWidth;
        set
        {
            ThrowIfClosed();

            if (_minWidth != value)
            {
                MinSize = MinSize with { X = value };
                _minWidth = value;
            }
        }
    }

    /// <summary>
    /// Gets the reference to parent PhotinoWindow instance.
    /// This property can only be set in the constructor and it is optional.
    /// </summary>
    public PhotinoWindow? Parent { get; }

    /// <summary>
    /// Gets or sets whether the native window can be resized by the user.
    /// Default is true.
    /// </summary>
    public bool Resizable
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Resizable;

            byte resizable = 0;
            Invoke(() => Photino_GetResizable(_nativeInstance, out resizable));
            return resizable != 0;
        }
        set
        {
            ThrowIfClosed();

            if (Resizable != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.Resizable = value;
                else
                    Invoke(() => Photino_SetResizable(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    /// <summary>
    /// Gets or sets the native window Size. This represents the width and the height of the window in pixels.
    /// The default Size is 0,0.
    /// </summary>
    /// <seealso cref="UseOsDefaultSize"/>
    public Size Size
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return new Size(_startupParameters.Width, _startupParameters.Height);

            var width = 0;
            var height = 0;
            Invoke(() => Photino_GetSize(_nativeInstance, out width, out height));
            return new Size(width, height);
        }
        set
        {
            ThrowIfClosed();

            if (Size.Width != value.Width || Size.Height != value.Height)
            {
                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.Height = value.Height;
                    _startupParameters.Width = value.Width;
                }
                else
                    Invoke(() => Photino_SetSize(_nativeInstance, value.Width, value.Height));
            }
        }
    }

    /// <summary>
    /// Gets or sets platform‑specific initialization parameters for the native browser control on startup.
    /// Default is none.
    /// </summary>
    /// <remarks>
    /// <para><b>Windows:</b> WebView2-specific argument string (space-separated).</para>
    /// <para>https://peter.sh/experiments/chromium-command-line-switches/</para>
    /// <para>https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2environmentoptions.additionalbrowserarguments</para>
    /// <para>https://www.chromium.org/developers/how-tos/run-chromium-with-flags/</para>
    ///
    /// <para><b>Linux:</b> WebKit2GTK JSON settings.</para>
    /// <para>Example: <c>{ "set_enable_encrypted_media": true }</c></para>
    /// <para>https://webkitgtk.org/reference/webkit2gtk/2.5.1/WebKitSettings.html</para>
    /// <para>https://lazka.github.io/pgi-docs/WebKit2-4.0/classes/Settings.html</para>
    ///
    /// <para><b>macOS:</b> WebKit (WKWebView) JSON settings.</para>
    /// <para>Example: <c>{ "minimumFontSize": 8 }</c></para>
    /// <para>https://developer.apple.com/documentation/webkit/wkwebviewconfiguration</para>
    /// <para>https://developer.apple.com/documentation/webkit/wkpreferences</para>
    /// </remarks>
    public string? BrowserControlInitParameters
    {
        get
        {
            return _startupParameters.BrowserControlInitParameters;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            var ss = _startupParameters.BrowserControlInitParameters;
            if (!string.Equals(ss, value, StringComparison.CurrentCultureIgnoreCase))
            {
                _startupParameters.BrowserControlInitParameters = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets an HTML string that the browser control will render when initialized.
    /// Default is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Either StartString or StartUrl must be specified.
    /// </remarks>
    /// <seealso cref="StartUrl" />
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    public string? StartString
    {
        get
        {
            return _startupParameters.StartString;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            var ss = _startupParameters.StartString;
            if (!string.Equals(ss, value, StringComparison.CurrentCultureIgnoreCase))
            {
                if (value != null)
                    LoadRawString(value);
                else
                    _startupParameters.StartString = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets an URL that the browser control will navigate to when initialized.
    /// Default is none.
    /// </summary>
    /// <remarks>
    /// Either StartString or StartUrl must be specified.
    /// </remarks>
    /// <seealso cref="StartString" />
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    public string? StartUrl
    {
        get
        {
            return _startupParameters.StartUrl;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            var su = _startupParameters.StartUrl;
            if (!string.Equals(su, value, StringComparison.CurrentCultureIgnoreCase))
            {
                if (value != null)
                    Load(value);
                else
                    _startupParameters.StartUrl = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the local path to store temp files for browser control.
    /// Default is the user's AppDataLocal folder.
    /// </summary>
    /// <remarks>
    /// Only available on Windows.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if platform is not Windows.
    /// </exception>
    public string? TemporaryFilesPath
    {
        get
        {
            return _startupParameters.TemporaryFilesPath;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            var tfp = _startupParameters.TemporaryFilesPath;
            if (tfp != value)
            {
                _startupParameters.TemporaryFilesPath = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the registration Id for doing toast notifications.
    /// Default is to use the window title.
    /// </summary>
    /// <remarks>
    /// Only available on Windows.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if platform is not Windows.
    /// </exception>
    public string? NotificationRegistrationId
    {
        get
        {
            return _startupParameters.NotificationRegistrationId;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            var nri = _startupParameters.NotificationRegistrationId;
            if (nri != value)
            {
                _startupParameters.NotificationRegistrationId = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the native window title.
    /// Default is <c>PhotinoX</c>.
    /// </summary>
    public string? Title
    {
        get => _title;
        set
        {
            ThrowIfClosed();

            if (_title != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.Title = value;
                    _title = value;
                }
                else
                {
                    Invoke(() =>
                    {
                        Photino_SetTitle(_nativeInstance, value);
                        var ptr = Photino_GetTitle(_nativeInstance);
                        try
                        {
                            _title = ptr != IntPtr.Zero
                                ? Marshal.PtrToStringUTF8(ptr)
                                : null;
                        }
                        finally
                        {
                            if (ptr != IntPtr.Zero)
                                Photino_FreeString(ptr);
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the native window Top (Y) coordinate in pixels.
    /// Default is 0.
    /// </summary>
    /// <seealso cref="UseOsDefaultLocation"/>
    public int Top
    {
        get => Location.Y;
        set
        {
            ThrowIfClosed();

            if (Location.Y != value)
                Location = Location with { Y = value };
        }
    }

    /// <summary>
    /// Gets or sets whether the native window is always at the top of the z-order.
    /// Default is false.
    /// </summary>
    public bool Topmost
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Topmost;

            byte topmost = 0;
            Invoke(() => Photino_GetTopmost(_nativeInstance, out topmost));
            return topmost != 0;
        }
        set
        {
            ThrowIfClosed();

            if (Topmost != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.Topmost = value;
                else
                    Invoke(() => Photino_SetTopmost(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    /// <summary>
    /// When true the native window starts up at the OS Default location.
    /// Default is true.
    /// </summary>
    /// <remarks>
    /// Overrides Left (X) and Top (Y) properties.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    public bool UseOsDefaultLocation
    {
        get
        {
            return _startupParameters.UseOsDefaultLocation;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            _startupParameters.UseOsDefaultLocation = value;
        }
    }

    /// <summary>
    /// When true the native window starts at the OS Default size.
    /// Default is true.
    /// </summary>
    /// <remarks>
    /// Overrides Height and Width properties.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    public bool UseOsDefaultSize
    {
        get
        {
            return _startupParameters.UseOsDefaultSize;
        }
        set
        {
            ThrowIfClosedOrInitialized();
            _startupParameters.UseOsDefaultSize = value;
        }
    }

    /// <summary>
    /// Gets or Sets the native window width in pixels.
    /// Default is 0.
    /// </summary>
    /// <seealso cref="UseOsDefaultSize"/>
    public int Width
    {
        get => Size.Width;
        set
        {
            ThrowIfClosed();

            var currentSize = Size;
            if (currentSize.Width != value)
                Size = currentSize with { Width = value };
        }
    }

    /// <summary>
    /// Gets or sets the native browser control <see cref="PhotinoWindow.Zoom"/>.
    /// Default is 100.
    /// </summary>
    /// <example>100 = 100%, 50 = 50%</example>
    public int Zoom
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Zoom;

            var zoom = 0;
            Invoke(() => Photino_GetZoom(_nativeInstance, out zoom));
            return zoom;
        }
        set
        {
            ThrowIfClosed();

            if (Zoom != value)
            {
                if (_nativeInstance == IntPtr.Zero)
                    _startupParameters.Zoom = value;
                else
                    Invoke(() => Photino_SetZoom(_nativeInstance, value));
            }
        }
    }

    /// <summary>
    /// Gets or sets the logging verbosity to standard output (Console/Terminal).
    /// 0 = Critical Only
    /// 1 = Critical and Warning
    /// 2 = Verbose
    /// >2 = All Details
    /// Default is 2.
    /// </summary>
    public int LogVerbosity { get; set; } = 2;

    //CONSTRUCTOR
    /// <summary>
    /// Initializes a new instance of the PhotinoWindow class.
    /// </summary>
    /// <remarks>
    /// This class represents a native window with a native browser control taking up the entire client area.
    /// If a parent window is specified, this window will be created as a child of the specified parent window.
    /// </remarks>
    /// <param name="parent">The parent PhotinoWindow. This is optional and defaults to null.</param>
    public PhotinoWindow(PhotinoWindow? parent = null)
    {
        Parent = parent;

        PhotinoBootstrap.Initialize();

        //Wire up handlers from C++ to C#
        _startupParameters.ClosingHandler = OnWindowClosing;
        _startupParameters.ResizedHandler = OnSizeChanged;
        _startupParameters.MaximizedHandler = OnMaximized;
        _startupParameters.RestoredHandler = OnRestored;
        _startupParameters.MinimizedHandler = OnMinimized;
        _startupParameters.MovedHandler = OnLocationChanged;
        _startupParameters.FocusInHandler = OnFocusIn;
        _startupParameters.FocusOutHandler = OnFocusOut;
        _startupParameters.WebMessageReceivedHandler = OnWebMessageReceived;
        _startupParameters.CustomSchemeHandler = OnCustomScheme;
        _startupParameters.ClosedHandler = OnWindowClosed;
    }

    /// <summary>
    /// Dispatches an Action to the UI thread if called from another thread.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="workItem">The delegate encapsulating a method / action to be executed in the UI thread.</param>
    public PhotinoWindow Invoke(Action workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        PhotinoApplication.Current.Dispatcher.Invoke(workItem);
        return this;
    }

    /// <summary>
    /// Loads a specified <see cref="Uri"/> into the browser control.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// Load() or LoadRawString() must be called before native window is initialized.
    /// </remarks>
    /// <param name="uri">A Uri pointing to the file or the URL to load.</param>
    public PhotinoWindow Load(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        Log($".Load({uri})");
        ThrowIfClosed();
        if (_nativeInstance == IntPtr.Zero)
            _startupParameters.StartUrl = uri.ToString();
        else
            Invoke(() => Photino_NavigateToUrl(_nativeInstance, uri.ToString()));
        return this;
    }

    /// <summary>
    /// Loads a specified path into the browser control.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <remarks>
    /// Load() or LoadRawString() must be called before native window is initialized.
    /// </remarks>
    /// <param name="path">A path pointing to the resource to load.</param>
    public PhotinoWindow Load(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        Log($".Load({path})");
        ThrowIfClosed();

        // ––––––––––––––––––––––
        // SECURITY RISK!
        // This needs validation!
        // ––––––––––––––––––––––
        // Open a web URL string path
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
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
    /// Used to load HTML into the browser control directly.
    /// Load() or LoadRawString() must be called before native window is initialized.
    /// </remarks>
    /// <param name="content">Raw content (such as HTML)</param>
    public PhotinoWindow LoadRawString(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var shortContent = content.Length > 50 ? string.Concat(content.AsSpan(0, 50), "...") : content;
        Log($".LoadRawString({shortContent})");
        ThrowIfClosed();
        if (_nativeInstance == IntPtr.Zero)
            _startupParameters.StartString = content;
        else
            Invoke(() => Photino_NavigateToString(_nativeInstance, content));
        return this;
    }

    /// <summary>
    /// Centers the native window on the primary display.
    /// </summary>
    /// <remarks>
    /// If called prior to window initialization, overrides Left (X) and Top (Y) properties.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <seealso cref="UseOsDefaultLocation" />
    public PhotinoWindow Center()
    {
        Log(".Center()");
        Centered = true;
        return this;
    }

    /// <summary>
    /// Restores the native window from a minimized or maximized state back to its previous size and position.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    public PhotinoWindow Restore()
    {
        Log(".Restore()");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_Restore(_nativeInstance));
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
        Log($".MoveTo({location}, {allowOutsideWorkArea})");

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
        if (Platform.IsMacOS && Platform.MacOS.IsPreSonoma)
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
        Log($".MoveTo({left}, {top}, {allowOutsideWorkArea})");
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
        Log($".Offset({offset})");
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
        Log($".Offset({left}, {top})");
        return Offset(new Point(left, top));
    }

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
        Log($".SetChromeless({chromeless})");
        Chromeless = chromeless;
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
        Log(".BeginWindowDrag()");
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
        Log($".BeginWindowResize({edge})");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_BeginWindowResize(_nativeInstance, edge));
        return this;
    }

    /// <summary>
    /// When true, the native window can be displayed with transparent background.
    /// Chromeless must be set to true. Html document's body background must have alpha-based value.
    /// By default, this is set to false.
    /// </summary>
    public PhotinoWindow SetTransparent(bool enabled)
    {
        Log($".SetTransparent({enabled})");
        Transparent = enabled;
        return this;
    }

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
        Log($".SetContextMenuEnabled({enabled})");
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
        Log($".SetZoomEnabled({enabled})");
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
        Log($".SetDevTools({enabled})");
        DevToolsEnabled = enabled;
        return this;
    }

    /// <summary>
    /// When set to true, the native window will cover the entire screen, similar to kiosk mode.
    /// By default, this is set to false.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="fullScreen">Whether the window should be fullscreen</param>
    public PhotinoWindow SetFullScreen(bool fullScreen)
    {
        Log($".SetFullScreen({fullScreen})");
        FullScreen = fullScreen;
        return this;
    }

    ///<summary>
    /// When set to true, the native browser control grants all requests for access to local resources
    /// such as the users camera and microphone. By default, this is set to true.
    /// </summary>
    /// <remarks>
    /// This only works on Windows.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="grant">Whether permissions should be automatically granted.</param>
    public PhotinoWindow SetGrantBrowserPermissions(bool grant)
    {
        Log($".SetGrantBrowserPermission({grant})");
        GrantBrowserPermissions = grant;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.UserAgent"/>. Sets the user agent on the browser control at initialization.
    /// </summary>
    /// <param name="userAgent"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetUserAgent(string userAgent)
    {
        Log($".SetUserAgent({userAgent})");
        UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.BrowserControlInitParameters"/> platform‑specific
    /// initialization parameters for the native browser control on startup.
    /// Default is none.
    /// </summary>
    /// <remarks>
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
        Log($".SetBrowserControlInitParameters({parameters})");
        BrowserControlInitParameters = parameters;
        return this;
    }

    /// <summary>
    /// Sets the registration id for toast notifications. 
    /// </summary>
    /// <remarks>
    /// Only available on Windows.
    /// Defaults to window title if not specified.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if platform is not Windows.
    /// </exception>
    /// <param name="notificationRegistrationId"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetNotificationRegistrationId(string notificationRegistrationId)
    {
        Log($".SetNotificationRegistrationId({notificationRegistrationId})");
        NotificationRegistrationId = notificationRegistrationId;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.MediaAutoplayEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetMediaAutoplayEnabled(bool enable)
    {
        Log($".SetMediaAutoplayEnabled({enable})");
        MediaAutoplayEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.FileSystemAccessEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetFileSystemAccessEnabled(bool enable)
    {
        Log($".SetFileSystemAccessEnabled({enable})");
        FileSystemAccessEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.WebSecurityEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetWebSecurityEnabled(bool enable)
    {
        Log($".SetWebSecurityEnabled({enable})");
        WebSecurityEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.JavascriptClipboardAccessEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetJavascriptClipboardAccessEnabled(bool enable)
    {
        Log($".SetJavascriptClipboardAccessEnabled({enable})");
        JavascriptClipboardAccessEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.MediaStreamEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetMediaStreamEnabled(bool enable)
    {
        Log($".SetMediaStreamEnabled({enable})");
        MediaStreamEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.SmoothScrollingEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetSmoothScrollingEnabled(bool enable)
    {
        Log($".SetSmoothScrollingEnabled({enable})");
        SmoothScrollingEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets <see cref="PhotinoWindow.IgnoreCertificateErrorsEnabled"/> on the browser control at initialization.
    /// </summary>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetIgnoreCertificateErrorsEnabled(bool enable)
    {
        Log($".SetIgnoreCertificateErrorsEnabled({enable})");
        IgnoreCertificateErrorsEnabled = enable;
        return this;
    }

    /// <summary>
    /// Sets whether ShowNotification() can be called.
    /// </summary>
    /// <remarks>
    /// Only available on Windows.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if platform is not Windows.
    /// </exception>
    /// <param name="enable"></param>
    /// <returns>Returns the current <see cref="PhotinoWindow"/> instance.</returns>
    public PhotinoWindow SetNotificationsEnabled(bool enable)
    {
        Log($".SetNotificationsEnabled({enable})");
        NotificationsEnabled = enable;
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
        Log($".SetHeight({height})");
        Height = height;
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
    /// <exception cref="System.ArgumentException">Icon file: {value} does not exist.</exception>
    /// <param name="iconFile">The file path to the icon.</param>
    public PhotinoWindow SetIconFile(string iconFile)
    {
        Log($".SetIconFile({iconFile})");
        IconFile = iconFile;
        return this;
    }

    /// <summary>
    /// Sets the icon file for the native window title bar from an embedded resource.
    /// The resource file is extracted to a temporary file, and its path is then set as the icon.
    /// </summary>
    /// <remarks>
    /// This only works on Windows and Linux.
    /// The resource file is expected to be embedded in the assembly from the `wwwroot` folder, and the provided namespace is used to locate the resource.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="resourceFileName">The name of the embedded resource file (e.g., "favicon.ico").</param>
    /// <param name="resourceNamespace">
    /// The namespace in which the embedded resource is located (e.g., "MyApp" or "MyCompany.MyApp").
    /// This allows for specifying the custom namespace where the resource is embedded.
    /// </param>
    public PhotinoWindow SetIconFile(string resourceFileName, string resourceNamespace)
    {
        var iconPath = ExtractEmbeddedResourceToTempFile(resourceFileName, resourceNamespace);
        return iconPath != null ? SetIconFile(iconPath) : this;
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
        Log($".SetLeft({left})");
        Left = left;
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
        Log($".SetResizable({resizable})");
        Resizable = resizable;
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
    /// <param name="size">Width &amp; Height</param>
    public PhotinoWindow SetSize(Size size)
    {
        Log($".SetSize({size})");
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
        Log($".SetSize({width}, {height})");
        Size = new Size(width, height);
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
        Log($".SetLocation({location})");
        Location = location;
        return this;
    }

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
        Log($".SetLogVerbosity({verbosity})");
        LogVerbosity = verbosity;
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
        Log($".SetMaximized({maximized})");
        Maximized = maximized;
        return this;
    }

    ///<summary>Native window maximum Width and Height in pixels.</summary>
    public PhotinoWindow SetMaxSize(int maxWidth, int maxHeight)
    {
        Log($".SetMaxSize({maxWidth}, {maxHeight})");
        MaxSize = new Point(maxWidth, maxHeight);
        return this;
    }

    ///<summary>Native window maximum Height in pixels.</summary>
    public PhotinoWindow SetMaxHeight(int maxHeight)
    {
        Log($".SetMaxHeight({maxHeight})");
        MaxHeight = maxHeight;
        return this;
    }

    ///<summary>Native window maximum Width in pixels.</summary>
    public PhotinoWindow SetMaxWidth(int maxWidth)
    {
        Log($".SetMaxWidth({maxWidth})");
        MaxWidth = maxWidth;
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
        Log($".SetMinimized({minimized})");
        Minimized = minimized;
        return this;
    }

    ///<summary>Native window maximum Width and Height in pixels.</summary>
    public PhotinoWindow SetMinSize(int minWidth, int minHeight)
    {
        Log($".SetMinSize({minWidth}, {minHeight})");
        MinSize = new Point(minWidth, minHeight);
        return this;
    }

    ///<summary>Native window maximum Height in pixels.</summary>
    public PhotinoWindow SetMinHeight(int minHeight)
    {
        Log($".SetMinHeight({minHeight})");
        MinHeight = minHeight;
        return this;
    }

    ///<summary>Native window maximum Width in pixels.</summary>
    public PhotinoWindow SetMinWidth(int minWidth)
    {
        Log($".SetMinWidth({minWidth})");
        MinWidth = minWidth;
        return this;
    }

    /// <summary>
    /// Sets the local path to store temp files for browser control.
    /// Default is the user's AppDataLocal folder.
    /// </summary>
    /// <remarks>
    /// Only available on Windows.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if platform is not Windows.
    /// </exception>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="tempFilesPath">Path to temp files directory.</param>
    public PhotinoWindow SetTemporaryFilesPath(string? tempFilesPath)
    {
        Log($".SetTemporaryFilesPath({tempFilesPath})");
        TemporaryFilesPath = tempFilesPath;
        return this;
    }

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
        Log($".SetTitle({title})");
        Title = title;
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
        Log($".SetTop({top})");
        Top = top;
        return this;
    }

    /// <summary>
    /// Sets whether the native window is always at the top of the z-order.
    /// Default is false.
    /// </summary>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="topMost">Whether the window is at the top</param>
    public PhotinoWindow SetTopMost(bool topMost)
    {
        Log($".SetTopMost({topMost})");
        Topmost = topMost;
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
        Log($".SetWidth({width})");
        Width = width;
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
        Log($".SetZoom({zoom})");
        Zoom = zoom;
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
        Log($".SetUseOsDefaultLocation({useOsDefault})");
        UseOsDefaultLocation = useOsDefault;
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
        Log($".SetUseOsDefaultSize({useOsDefault})");
        UseOsDefaultSize = useOsDefault;
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
    /// <param name="data">Runtime path for WebView2</param>
    public PhotinoWindow Win32SetWebView2Path(string data)
    {
        if (Platform.IsWindows)
            Photino_setWebView2RuntimePath_win32(data);
        else
            Log("Win32SetWebView2Path is only supported on the Windows platform");

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
        ThrowIfClosedOrNotInitialized();
        if (Platform.IsWindows)
            Invoke(() => Photino_ClearBrowserAutoFill(_nativeInstance));
        else
            Log("ClearBrowserAutoFill is only supported on the Windows platform");

        return this;
    }

    /// <summary>
    /// Attempts to activate the native Photino window.
    /// </summary>
    /// <returns><c>true</c> if the window was activated; otherwise, <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    public bool Activate()
    {
        Log(".Activate()");
        ThrowIfClosedOrNotInitialized();
        var activated = false;
        Invoke(() => activated = Photino_Activate(_nativeInstance));
        return activated;
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
        ThrowIfClosed();
        Show();
        if (Minimized)
            Restore();
        Activate();
        return this;
    }

    /// <summary>
    /// Creates and shows the native Photino window.
    /// </summary>
    /// <remarks>
    /// If the native window has already been created, this method shows the existing window.
    /// A closed window cannot be shown again.
    /// </remarks>
    public void Show()
    {
        ThrowIfClosed();

        if (_nativeInstance != IntPtr.Zero)
        {
            Invoke(() => Photino_Show(_nativeInstance));
            return;
        }

        if (Platform.IsWindows && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            ThrowWindowMustBeCreatedOnStaThread();

        PhotinoApplication.Current.Dispatcher.VerifyAccessToCreateWindow();

        // 1. Fill fixed-size array of custom scheme names
        Array.Clear(_startupParameters.CustomSchemeNames);
        var i = 0;
        foreach (var pair in CustomSchemes)
        {
            var scheme = pair.Key;
            if (!IsValidSchemeName(scheme))
                continue;
            _startupParameters.CustomSchemeNames[i++] = scheme;
            if (i == _startupParameters.CustomSchemeNames.Length)
                break;
        }

        _startupParameters.Title = _title;
        _startupParameters.NativeParent = Parent?._nativeInstance ?? IntPtr.Zero;

        // 2. Validate startup parameters
        List<string>? errors = null;
        _startupParameters.GetParamErrors(ref errors);
        if (errors is { Count: > 0 })
        {
            throw new ArgumentException($"Startup parameters are not valid:{Environment.NewLine}" +
                                        string.Join(Environment.NewLine, errors.Select(e => $" - {e}")));
        }

        // 3. Create native window
        OnWindowCreating();
        try
        {
            _nativeInstance = Photino_ctor(ref _startupParameters);
            if (_nativeInstance == IntPtr.Zero)
                throw new ExternalException("Native window creation failed.");
        }
        catch (Exception ex)
        {
            int lastError = 0;
            if (Platform.IsWindows)
                lastError = Marshal.GetLastWin32Error();

            Log($"Error #{lastError}{Environment.NewLine}{ex}");
            throw new ExternalException(
                $"Native code exception. Error # {lastError}. See inner exception for details.", ex)
            { HResult = lastError };
        }

        OnWindowCreated();
    }

    /// <summary>
    /// Closes the native window.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    public void Close()
    {
        Log(".Close()");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_Close(_nativeInstance));
    }

    internal void InternalClose()
    {
        if (_nativeInstance == IntPtr.Zero)
            return;

        _suppressClosing = true;
        Invoke(() => Photino_Close(_nativeInstance));
    }

    /// <summary>
    /// Send a message to the native window's native browser control's JavaScript context.
    /// </summary>
    /// <remarks>
    /// In JavaScript, messages can be received via <code>window.external.receiveMessage(message)</code>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    /// <param name="message">Message as string</param>
    public void SendWebMessage(string message)
    {
        Log($".SendWebMessage({message})");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_SendWebMessage(_nativeInstance, message));
    }

    public Task SendWebMessageAsync(string message)
    {
        return Task.Run(() =>
        {
            SendWebMessage(message);
        });
    }

    /// <summary>
    /// Sends a native notification to the OS.
    /// Sometimes referred to as Toast notifications.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    /// <param name="title">The title of the notification</param>
    /// <param name="body">The text of the notification</param>
    public void SendNotification(string title, string body)
    {
        Log($".SendNotification({title}, {body})");
        ThrowIfClosedOrNotInitialized();
        Invoke(() => Photino_ShowNotification(_nativeInstance, title, body));
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <param name="message">Log message</param>
    internal void Log(string message)
    {
        if (LogVerbosity < 1) return;
        Console.WriteLine($"PhotinoX: \"{Title ?? DefaultTitle}\"{message}");
    }

    /// <summary>
    /// Extracts an embedded resource from the assembly to a temporary file.
    /// </summary>
    /// <remarks>
    /// The resource is expected to be located within the provided namespace and under the `wwwroot` folder.
    /// This method will write the resource to a temporary file and return its path.
    /// </remarks>
    /// <returns>
    /// The path to the temporary file containing the extracted resource, or <c>null</c> if the resource was not found.
    /// </returns>
    /// <param name="fileName">The name of the embedded resource file (e.g., "favicon.ico").</param>
    /// <param name="resourceNamespace">
    /// The namespace where the embedded resource is located (e.g., "MyApp" or "MyCompany.MyApp").
    ///
    /// The method expects the resource to be in the `wwwroot` folder of the provided namespace.
    /// </param>
    private string? ExtractEmbeddedResourceToTempFile(string fileName, string resourceNamespace)
    {
        string resourceName = $"{resourceNamespace}.wwwroot.{fileName}";

        Assembly assembly = Assembly.GetExecutingAssembly();

        using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            Log($"Resource '{fileName}' couldn't be found in namespace '{resourceNamespace}'");
            return null;
        }

        string tempFile = Path.Combine(Path.GetTempPath(), fileName);

        using FileStream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
        resourceStream.CopyTo(fileStream);

        return tempFile;
    }

    private static void ThrowWindowMustBeCreatedOnStaThread()
    {
        throw new InvalidOperationException("A Photino window must be created on an STA thread on Windows.");
    }

    private void ThrowIfClosedOrNotInitialized([CallerMemberName] string? callerName = null)
    {
        ThrowIfClosed(callerName);
        ThrowIfNotInitialized(callerName);
    }

    private void ThrowIfClosed([CallerMemberName] string? callerName = null)
    {
        if (IsClosed)
            ThrowWindowAlreadyClosed(callerName);
    }

    private void ThrowIfNotInitialized([CallerMemberName] string? callerName = null)
    {
        if (_nativeInstance == IntPtr.Zero)
            ThrowWindowNotInitialized(callerName);
    }

    private void ThrowIfClosedOrInitialized([CallerMemberName] string? memberName = null)
    {
        ThrowIfClosed(memberName);

        if (_nativeInstance != IntPtr.Zero)
            ThrowWindowAlreadyInitialized(memberName);
    }

    [DoesNotReturn]
    private static void ThrowWindowAlreadyClosed(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot be called after the Photino window has been closed.");
    }

    [DoesNotReturn]
    private static void ThrowWindowNotInitialized(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot be called until after the Photino window is initialized.");
    }

    [DoesNotReturn]
    private static void ThrowWindowAlreadyInitialized(string? memberName)
    {
        throw new InvalidOperationException($"{memberName} can only be set before the Photino window is initialized.");
    }
}
