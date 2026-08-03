using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

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
        WindowState = PhotinoWindowState.Normal,
        ChromelessResizeBorderThickness = 8
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

            IntPtr handle;
            if (Platform.IsWindows)
            {
                handle = Dispatcher.Invoke(static nativeInstance => Photino_getHwnd_win32(nativeInstance), _nativeInstance);
                return handle;
            }

            if (Platform.IsLinux)
            {
                handle = Dispatcher.Invoke(static nativeInstance => Photino_getGtkWidget_linux(nativeInstance), _nativeInstance);
                return handle;
            }

            if (Platform.IsMacOS)
            {
                handle = Dispatcher.Invoke(static nativeInstance => Photino_getNSWindow_mac(nativeInstance), _nativeInstance);
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

            var state = new GetMonitorsState
            {
                NativeInstance = _nativeInstance,
                Monitors = []
            };

            bool enumerated = Dispatcher.Invoke(static state =>
            {
                using var scope = new GCHandleScope(state, out var stateHandle);
                return Photino_GetAllMonitors(state.NativeInstance, s_getAllMonitorsCallback, stateHandle);
            }, state);
            Debug.Assert(enumerated);

            if (!enumerated)
                throw new InvalidOperationException("Failed to enumerate native monitors.");

            return state.Monitors;
        }
    }

    /// <summary>
    /// Gets information about the monitor that currently contains the native window.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized, has already been closed,
    /// or the native window monitor cannot be resolved.
    /// </exception>
    /// <returns>
    /// Returns a <see cref="Monitor"/> object representing the monitor that currently contains the native window.
    /// </returns>
    public Monitor MainMonitor
    {
        get
        {
            ThrowIfClosedOrNotInitialized();

            return Dispatcher.Invoke(static nativeInstance =>
            {
                if (!Photino_GetWindowMonitor(nativeInstance, out NativeMonitor monitor))
                    throw new InvalidOperationException("Failed to get the native window monitor.");

                return new Monitor(monitor);
            }, _nativeInstance);
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

            uint dpi = Dispatcher.Invoke(static nativeInstance => Photino_GetScreenDpi(nativeInstance), _nativeInstance);
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
            if (value)
                _startupParameters.UseOsDefaultLocation = false;
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
    /// Gets or sets Linux-only native hit-test settings for chromeless windows.
    /// </summary>
    /// <remarks>
    /// These settings are ignored on Windows and macOS.
    /// </remarks>
    public Platform.Linux.ChromelessSettings LinuxChromelessSettings
    {
        get
        {
            return new Platform.Linux.ChromelessSettings(
                dragRegionHeight: _startupParameters.ChromelessDragRegionHeight,
                dragRegionLeftInset: _startupParameters.ChromelessDragRegionLeftInset,
                dragRegionRightInset: _startupParameters.ChromelessDragRegionRightInset,
                resizeBorderThickness: _startupParameters.ChromelessResizeBorderThickness);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.ChromelessDragRegionHeight = value.DragRegionHeight;
            _startupParameters.ChromelessDragRegionLeftInset = value.DragRegionLeftInset;
            _startupParameters.ChromelessDragRegionRightInset = value.DragRegionRightInset;
            _startupParameters.ChromelessResizeBorderThickness = value.ResizeBorderThickness;
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetTransparentEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Transparent = value;
                return;
            }

            if (Platform.IsWindows)
                throw new InvalidOperationException("Transparent can only be set on Windows before the native window is instantiated.");

            Log($"Invoking {nameof(Photino_SetTransparentEnabled)}({value})");

            Dispatcher.Invoke(static state =>
            {
                Photino_SetTransparentEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetContextMenuEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.ContextMenuEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetContextMenuEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetZoomEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.ZoomEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetZoomEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetDevToolsEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.DevToolsEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetDevToolsEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
        }
    }

    public bool MediaAutoplayEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.MediaAutoplayEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetMediaAutoplayEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.MediaAutoplayEnabled = value;
        }
    }

    public string? UserAgent
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.UserAgent;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                var ptr = Photino_GetUserAgent(nativeInstance);
                try
                {
                    return ptr != IntPtr.Zero
                        ? Marshal.PtrToStringUTF8(ptr)
                        : null;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Photino_FreeString(ptr);
                }
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.UserAgent = value;
        }
    }

    public bool FileSystemAccessEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.FileSystemAccessEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetFileSystemAccessEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.FileSystemAccessEnabled = value;
        }
    }

    public bool WebSecurityEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.WebSecurityEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetWebSecurityEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.WebSecurityEnabled = value;
        }
    }

    public bool JavascriptClipboardAccessEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.JavascriptClipboardAccessEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetJavascriptClipboardAccessEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.JavascriptClipboardAccessEnabled = value;
        }
    }

    public bool MediaStreamEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.MediaStreamEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetMediaStreamEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.MediaStreamEnabled = value;
        }
    }

    public bool SmoothScrollingEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.SmoothScrollingEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetSmoothScrollingEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.SmoothScrollingEnabled = value;
        }
    }

    public bool IgnoreCertificateErrorsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.IgnoreCertificateErrorsEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetIgnoreCertificateErrorsEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.IgnoreCertificateErrorsEnabled = value;
        }
    }

    public bool NotificationsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.NotificationsEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetNotificationsEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.NotificationsEnabled = value;
        }
    }

    /// <summary>
    /// Gets or sets whether this window should use a native owner relationship with its logical parent where supported.
    /// Currently supported on Windows only. Default is false.
    /// </summary>
    public bool UseNativeWindowOwner
    {
        get => _startupParameters.UseNativeWindowOwner;
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.UseNativeWindowOwner = value;
        }
    }

    /// <summary>
    /// Gets or sets the native window state.
    /// </summary>
    public PhotinoWindowState WindowState
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.WindowState;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetWindowState(nativeInstance, out PhotinoWindowState state);
                return state;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            ThrowIfNotValidWindowState(value);

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.WindowState = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetWindowState(state.NativeInstance, state.Value);
            }, (NativeInstance: _nativeInstance, Value: value));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetGrantBrowserPermissions(nativeInstance, out byte grant);
                return grant != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.GrantBrowserPermissions = value;
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
    /// Thrown when the icon file path does not reference an existing file.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when trying to clear the icon after the native window is initialized.
    /// </exception>
    public string? IconFile
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.WindowIconFile;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                var ptr = Photino_GetIconFile(nativeInstance);
                try
                {
                    return ptr != IntPtr.Zero
                        ? Marshal.PtrToStringUTF8(ptr)
                        : null;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Photino_FreeString(ptr);
                }
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (string.IsNullOrWhiteSpace(value))
            {
                if (_nativeInstance == IntPtr.Zero)
                {
                    _startupParameters.WindowIconFile = null;
                    return;
                }

                throw new InvalidOperationException("IconFile can only be cleared before the native window is initialized.");
            }

            var iconFile = value;

            if (!File.Exists(iconFile))
            {
                iconFile = Path.Combine(AppContext.BaseDirectory, value);

                if (!File.Exists(iconFile))
                    throw new ArgumentException($"Icon file: {value} does not exist.", nameof(value));
            }

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.WindowIconFile = iconFile;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetIconFile(state.NativeInstance, state.IconFile);
            }, (NativeInstance: _nativeInstance, IconFile: iconFile));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetPosition(nativeInstance, out int left, out int top);
                return new Point(left, top);
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Left = value.X;
                _startupParameters.Top = value.Y;
                _startupParameters.UseOsDefaultLocation = false;
                _startupParameters.CenterOnInitialize = false;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetPosition(state.NativeInstance, state.X, state.Y);
            }, (NativeInstance: _nativeInstance, X: value.X, Y: value.Y));
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

            if (MaxWidth == value.X && MaxHeight == value.Y)
                return;

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.MaxWidth = value.X;
                _startupParameters.MaxHeight = value.Y;
            }
            else
            {
                Dispatcher.Invoke(static state =>
                {
                    Photino_SetMaxSize(state.NativeInstance, state.Width, state.Height);
                }, (NativeInstance: _nativeInstance, Width: value.X, Height: value.Y));
            }

            _maxWidth = value.X;
            _maxHeight = value.Y;
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

            if (MinWidth == value.X && MinHeight == value.Y)
                return;

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.MinWidth = value.X;
                _startupParameters.MinHeight = value.Y;
            }
            else
            {
                Dispatcher.Invoke(static state =>
                {
                    Photino_SetMinSize(state.NativeInstance, state.Width, state.Height);
                }, (NativeInstance: _nativeInstance, Width: value.X, Height: value.Y));
            }

            _minWidth = value.X;
            _minHeight = value.Y;
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetResizable(nativeInstance, out byte resizable);
                return resizable != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Resizable = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetResizable(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetSize(nativeInstance, out int width, out int height);
                return new Size(width, height);
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Width = value.Width;
                _startupParameters.Height = value.Height;
                _startupParameters.UseOsDefaultSize = false;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetSize(state.NativeInstance, state.Width, state.Height);
            }, (NativeInstance: _nativeInstance, Width: value.Width, Height: value.Height));
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

            _startupParameters.BrowserControlInitParameters = value;
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

            _startupParameters.StartString = value;
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

            _startupParameters.StartUrl = value;
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

            _startupParameters.TemporaryFilesPath = value;
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

            _startupParameters.NotificationRegistrationId = value;
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

            if (_title == value)
                return;

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Title = value;
                _title = value;
                return;
            }

            _title = Dispatcher.Invoke(static state =>
            {
                Photino_SetTitle(state.NativeInstance, state.Value);

                var ptr = Photino_GetTitle(state.NativeInstance);
                try
                {
                    return ptr != IntPtr.Zero
                        ? Marshal.PtrToStringUTF8(ptr)
                        : null;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Photino_FreeString(ptr);
                }
            }, (NativeInstance: _nativeInstance, Value: value));
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetTopmost(nativeInstance, out byte topmost);
                return topmost != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Topmost = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetTopmost(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
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
            if (value)
                _startupParameters.CenterOnInitialize = false;
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

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetZoom(nativeInstance, out int zoom);
                return zoom;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Zoom = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetZoom(state.NativeInstance, state.Value);
            }, (NativeInstance: _nativeInstance, Value: value));
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
    /// If a parent window is specified, it is used as a logical parent.
    /// Native owner behavior is controlled separately by <see cref="UseNativeWindowOwner"/>.
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
        _startupParameters.ContentLoadedHandler = OnContentLoaded;
        _startupParameters.NavigationStartingHandler = OnNavigationStarting;
        _startupParameters.NewWindowRequestedHandler = OnNewWindowRequested;
        _startupParameters.CustomSchemeHandler = OnCustomScheme;
        _startupParameters.ClosedHandler = OnClosed;
        _startupParameters.FullScreenChangedHandler = OnFullScreenChanged;
        _startupParameters.StateChangedHandler = OnStateChanged;
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
        Log($".{nameof(Activate)}()");
        ThrowIfClosedOrNotInitialized();

        return Dispatcher.Invoke(
            static nativeInstance => Photino_Activate(nativeInstance),
            _nativeInstance);
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
            Dispatcher.Invoke(static nativeInstance => Photino_Show(nativeInstance), _nativeInstance);
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
            throw new ExternalException($"Native code exception. Error # {lastError}. See inner exception for details.", ex) { HResult = lastError };
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
        Log($".{nameof(Close)}()");
        ThrowIfClosedOrNotInitialized();

        Dispatcher.Invoke(static nativeInstance => Photino_Close(nativeInstance), _nativeInstance);
    }

    internal void InternalClose()
    {
        if (_nativeInstance == IntPtr.Zero)
            return;

        _suppressClosing = true;

        Dispatcher.Invoke(static nativeInstance => Photino_Close(nativeInstance), _nativeInstance);
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

        _startupParameters.Title = _title ?? DefaultTitle;
        _startupParameters.NativeParent = Parent?._nativeInstance ?? IntPtr.Zero;

        _startupParameters.Size = Marshal.SizeOf<PhotinoNativeParameters>();
        Debug.Assert(_startupParameters.Size == 416);
        _startupParameters.AbiVersion = PhotinoNativeParameters.NativeAbiVersion;

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
        Log($".{nameof(SendWebMessage)}({message})");
        ThrowIfClosedOrNotInitialized();

        Dispatcher.Invoke(static state =>
        {
            Photino_SendWebMessage(state.NativeInstance, state.Message);
        }, (NativeInstance: _nativeInstance, Message: message));
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
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous send operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window is not initialized or has already been closed.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is canceled.
    /// </exception>
    public Task SendWebMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        Log($".{nameof(SendWebMessageAsync)}({message})");
        ThrowIfClosedOrNotInitialized();

        return Dispatcher.InvokeAsync(static state =>
        {
            Photino_SendWebMessage(state.NativeInstance, state.Message);
        }, (NativeInstance: _nativeInstance, Message: message), cancellationToken);
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
        Log($".{nameof(SendNotification)}({title}, {body})");
        ThrowIfClosedOrNotInitialized();

        Dispatcher.Invoke(static state =>
        {
            Photino_ShowNotification(state.NativeInstance, state.Title, state.Body);
        }, (NativeInstance: _nativeInstance, Title: title, Body: body));
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
