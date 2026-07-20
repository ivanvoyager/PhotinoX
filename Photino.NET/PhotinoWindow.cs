using System.Drawing;
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
    private bool _isCreating;
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
    /// Gets or sets whether the native window should be centered when it is initialized.
    /// Default is false.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when trying to set the value after the native window is initialized, or after it has been closed.
    /// </exception>
    public bool CenterOnInitialize
    {
        get => _startupParameters.CenterOnInitialize;
        set
        {
            ThrowIfClosedOrInitialized();
            _startupParameters.CenterOnInitialize = value;
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.Transparent = value;
            else
            {
                if (Platform.IsWindows)
                    throw new InvalidOperationException("Transparent can only be set on Windows before the native window is instantiated.");

                Log($"Invoking Photino_SetTransparentEnabled({value})");
                Invoke(() => Photino_SetTransparentEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.ContextMenuEnabled = value;
            else
                Invoke(() => Photino_SetContextMenuEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.ZoomEnabled = value;
            else
                Invoke(() => Photino_SetZoomEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.DevToolsEnabled = value;
            else
                Invoke(() => Photino_SetDevToolsEnabled(_nativeInstance, (byte)(value ? 1 : 0)));
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
    public bool FullScreen // TODO remove
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

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.FullScreen = value;
                if (value)
                {
                    _startupParameters.Maximized = false;
                    _startupParameters.Minimized = false;
                }
            }
            else
            {
                Invoke(() => Photino_SetFullScreen(_nativeInstance, (byte)(value ? 1 : 0)));
            }
        }
    }

    /// <summary>
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

    /// <summary>
    /// Gets or sets the native window height in pixels.
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
    /// <exception cref="ArgumentException">
    /// Thrown when the icon file path is null, empty, whitespace, or does not reference an existing file.
    /// </exception>
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

            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            var iconFile = value;

            if (!File.Exists(iconFile))
            {
                iconFile = Path.Combine(AppContext.BaseDirectory, value);

                if (!File.Exists(iconFile))
                    throw new ArgumentException($"Icon file: {value} does not exist.");
            }

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.WindowIconFile = iconFile;
            else
                Invoke(() => Photino_SetIconFile(_nativeInstance, iconFile));
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

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Left = value.X;
                _startupParameters.Top = value.Y;
            }
            else
                Invoke(() => Photino_SetPosition(_nativeInstance, value.X, value.Y));
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

            var location = Location;
            if (location.X != value)
                Location = location with { X = value };
        }
    }

    /// <summary>
    /// Gets or sets the maximum size of the native window in pixels.
    /// </summary>
    public Point MaxSize
    {
        get => new(MaxWidth, MaxHeight);
        set
        {
            ThrowIfClosed();

            if (MaxWidth != value.X || MaxHeight != value.Y)
            {
                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.MaxWidth = value.X;
                    _startupParameters.MaxHeight = value.Y;
                }
                else
                    Invoke(() => Photino_SetMaxSize(_nativeInstance, value.X, value.Y));

                _maxWidth = value.X;
                _maxHeight = value.Y;
            }
        }
    }

    private int _maxHeight = int.MaxValue;

    /// <summary>
    /// Gets or sets the native window maximum height in pixels.
    /// </summary>
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

    private int _maxWidth = int.MaxValue;

    /// <summary>
    /// Gets or sets the native window maximum width in pixels.
    /// </summary>
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
    /// Gets or sets the minimum size of the native window in pixels.
    /// </summary>
    public Point MinSize
    {
        get => new(MinWidth, MinHeight);
        set
        {
            ThrowIfClosed();

            if (MinWidth != value.X || MinHeight != value.Y)
            {
                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.MinWidth = value.X;
                    _startupParameters.MinHeight = value.Y;
                }
                else
                    Invoke(() => Photino_SetMinSize(_nativeInstance, value.X, value.Y));

                _minWidth = value.X;
                _minHeight = value.Y;
            }
        }
    }

    private int _minHeight;

    /// <summary>
    /// Gets or sets the native window minimum height in pixels.
    /// </summary>
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

    private int _minWidth;

    /// <summary>
    /// Gets or sets the native window minimum width in pixels.
    /// </summary>
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.Resizable = value;
            else
                Invoke(() => Photino_SetResizable(_nativeInstance, (byte)(value ? 1 : 0)));
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

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Height = value.Height;
                _startupParameters.Width = value.Width;
            }
            else
                Invoke(() => Photino_SetSize(_nativeInstance, value.Width, value.Height));
        }
    }

    /// <summary>
    /// Gets or sets platform‑specific initialization parameters for the native browser control on startup.
    /// Default is none.
    /// </summary>
    /// <remarks>
    /// The value is passed to the native browser backend during initialization.
    /// Supported format and options are platform-specific.
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
            if (!string.Equals(ss, value, StringComparison.Ordinal))
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
            if (!string.Equals(ss, value, StringComparison.Ordinal))
            {
                if (value != null)
                    LoadString(value);
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
            if (!string.Equals(su, value, StringComparison.Ordinal))
            {
                if (value != null)
                    Load(new Uri(value, UriKind.Absolute));
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

            var location = Location;
            if (location.Y != value)
                Location = location with { Y = value };
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.Topmost = value;
            else
                Invoke(() => Photino_SetTopmost(_nativeInstance, (byte)(value ? 1 : 0)));
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

            if (_nativeInstance == IntPtr.Zero)
                _startupParameters.Zoom = value;
            else
                Invoke(() => Photino_SetZoom(_nativeInstance, value));
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
        _startupParameters.ClosingHandler = OnClosing;
        _startupParameters.ResizedHandler = OnSizeChanged;
        _startupParameters.MaximizedHandler = OnMaximized;
        _startupParameters.RestoredHandler = OnRestored;
        _startupParameters.MinimizedHandler = OnMinimized;
        _startupParameters.MovedHandler = OnLocationChanged;
        _startupParameters.FocusInHandler = OnActivated;
        _startupParameters.FocusOutHandler = OnDeactivated;
        _startupParameters.WebMessageReceivedHandler = OnWebMessageReceived;
        _startupParameters.CustomSchemeHandler = OnCustomScheme;
        _startupParameters.ClosedHandler = OnClosed;
        _startupParameters.FullScreenChangedHandler = OnFullScreenChanged;
    }

    /// <summary>
    /// Gets the dispatcher associated with the current Photino application.
    /// </summary>
    /// <remarks>
    /// Use this dispatcher to marshal work back to the UI thread.
    /// </remarks>
    public PhotinoDispatcher Dispatcher => PhotinoApplication.Current.Dispatcher;

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
        Dispatcher.Invoke(workItem);
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

        ThrowIfCreating();

        if (Platform.IsWindows && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            ThrowWindowMustBeCreatedOnStaThread();

        Dispatcher.VerifyAccessToCreateWindow();

        _isCreating = true;
        try
        {
            OnCreating();
        }
        finally
        {
            _isCreating = false;
        }

        PrepareAndValidateStartupParameters();
        ThrowIfInitializedForCall();
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

        if (_nativeInstance != IntPtr.Zero)
            OnCreated();
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

    private void PrepareAndValidateStartupParameters()
    {
        // Fill fixed-size array of custom scheme names
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

        // Validate startup parameters
        List<string>? errors = null;
        _startupParameters.GetParamErrors(ref errors);
        if (errors is { Count: > 0 })
        {
            throw new ArgumentException($"Startup parameters are not valid:{Environment.NewLine}" +
                                        string.Join(Environment.NewLine, errors.Select(e => $" - {e}")));
        }
    }

    /// <summary>
    /// Sends a message to the native browser control's JavaScript context.
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

    /// <summary>
    /// Sends a message asynchronously to the native browser control's JavaScript context.
    /// </summary>
    /// <remarks>
    /// In JavaScript, messages can be received via <code>window.external.receiveMessage(message)</code>.
    /// </remarks>
    /// <param name="message">
    /// The message to send.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous send operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    public Task SendWebMessageAsync(string message)
    {
        return Task.Run(() =>
        {
            SendWebMessage(message);
        });
    }

    /// <summary>
    /// Sends a native notification through the operating system.
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
}
