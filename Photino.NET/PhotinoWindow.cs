using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

using static Photino.NET.NativeMethods;

namespace Photino.NET;
/// <summary>
/// The PhotinoWindow class represents a window in a Photino-based desktop application.
/// </summary>
public partial class PhotinoWindow
{
    /// <summary>
    /// Parameters sent to Photino.Native to start a new instance of a Photino.Native window.
    /// </summary>

    private PhotinoWindowNativeParameters _startupParameters = new()
    {
        Size = Marshal.SizeOf<PhotinoWindowNativeParameters>(),
        AbiVersion = PhotinoWindowNativeParameters.NativeAbiVersion,

        Window = new()
        {
            Title = DefaultTitle
        },

        LinuxChromeless = new()
        {
            ResizeBorderThickness = 8
        },

        Geometry = new()
        {
            MaxHeight = int.MaxValue,
            MaxWidth = int.MaxValue,
            WindowState = PhotinoWindowState.Normal,

            Resizable = true,
            UseOsDefaultLocation = true,
            UseOsDefaultSize = true
        },

        Browser = new()
        {
            UserDataFolder = Platform.IsWindows
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PhotinoX")
                : null,
            UserAgent = "PhotinoX WebView",
            CustomSchemeNames = new string[PhotinoWindowNativeParameters.MaxCustomSchemeNames],

            Zoom = 100,
            ZoomEnabled = true,
            ContextMenuEnabled = true,
            StatusBarEnabled = true,
            DevToolsEnabled = true,

            GrantBrowserPermissions = true,
            MediaAutoplayEnabled = true,
            FileSystemAccessEnabled = true,
            WebSecurityEnabled = true,
            JavascriptClipboardAccessEnabled = true,
            MediaStreamEnabled = true,
            SmoothScrollingEnabled = true,
            IgnoreCertificateErrorsEnabled = false
        }
    };

    private const string DefaultTitle = "PhotinoX";

    private IntPtr _nativeInstance;
    private bool _isCreating;
    private bool _suppressClosing;

    private string? _title = DefaultTitle;

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
        _startupParameters.Callbacks = new()
        {
            ClosingHandler = OnClosing,
            ResizedHandler = OnSizeChanged,
            MaximizedHandler = OnMaximized,
            RestoredHandler = OnRestored,
            MinimizedHandler = OnMinimized,
            MovedHandler = OnLocationChanged,
            FocusInHandler = OnActivated,
            FocusOutHandler = OnDeactivated,
            WebMessageReceivedHandler = OnWebMessageReceived,
            ContentLoadingHandler = OnContentLoading,
            ContentLoadedHandler = OnContentLoaded,
            NavigationStartingHandler = OnNavigationStarting,
            NewWindowRequestedHandler = OnNewWindowRequested,
            CustomSchemeHandler = OnCustomScheme,
            ClosedHandler = OnClosed,
            FullScreenChangedHandler = OnFullScreenChanged,
            StateChangedHandler = OnStateChanged
        };
    }

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
        get => _startupParameters.Geometry.CenterOnInitialize;
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Geometry.CenterOnInitialize = value;
            if (value)
                _startupParameters.Geometry.UseOsDefaultLocation = false;
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
            return _startupParameters.Window.Chromeless;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Window.Chromeless = value;
        }
    }

    /// <summary>
    /// Gets or sets the initial Linux-only native hit-test settings for chromeless windows.
    /// </summary>
    /// <remarks>
    /// These settings are ignored on Windows and macOS and can only be changed before
    /// native window initialization. Use <see cref="SetLinuxChromelessDragRegion(int, int, int, int)"/>,
    /// <see cref="SetLinuxChromelessDragRegions(IReadOnlyList{LayoutRegion}, IReadOnlyList{LayoutRegion}?)"/>,
    /// and <see cref="SetLinuxChromelessResizeBorderThickness(int)"/> for runtime updates.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when setting the value after native window initialization or after the window has been closed.
    /// </exception>
    public Platform.Linux.ChromelessSettings LinuxChromelessSettings
    {
        get
        {
            return new Platform.Linux.ChromelessSettings(
                dragRegionHeight: _startupParameters.LinuxChromeless.DragRegionHeight,
                dragRegionLeftInset: _startupParameters.LinuxChromeless.DragRegionLeftInset,
                dragRegionTopInset: _startupParameters.LinuxChromeless.DragRegionTopInset,
                dragRegionRightInset: _startupParameters.LinuxChromeless.DragRegionRightInset,
                resizeBorderThickness: _startupParameters.LinuxChromeless.ResizeBorderThickness);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.LinuxChromeless.DragRegionHeight = value.DragRegionHeight;
            _startupParameters.LinuxChromeless.DragRegionLeftInset = value.DragRegionLeftInset;
            _startupParameters.LinuxChromeless.DragRegionTopInset = value.DragRegionTopInset;
            _startupParameters.LinuxChromeless.DragRegionRightInset = value.DragRegionRightInset;
            _startupParameters.LinuxChromeless.ResizeBorderThickness = value.ResizeBorderThickness;
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
                return _startupParameters.Window.Transparent;

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
                _startupParameters.Window.Transparent = value;
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
    /// Gets or sets whether this window should use a native owner relationship with its logical parent where supported.
    /// Currently supported on Windows only. Default is false.
    /// </summary>
    public bool UseNativeWindowOwner
    {
        get => _startupParameters.Window.UseNativeWindowOwner;
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Window.UseNativeWindowOwner = value;
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
                return _startupParameters.Geometry.WindowState;

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
                _startupParameters.Geometry.WindowState = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetWindowState(state.NativeInstance, state.Value);
            }, (NativeInstance: _nativeInstance, Value: value));
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
                return _startupParameters.Window.IconFile;

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
                    _startupParameters.Window.IconFile = null;
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
                _startupParameters.Window.IconFile = iconFile;
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
                return new Point(_startupParameters.Geometry.Left, _startupParameters.Geometry.Top);

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
                _startupParameters.Geometry.Left = value.X;
                _startupParameters.Geometry.Top = value.Y;
                _startupParameters.Geometry.UseOsDefaultLocation = false;
                _startupParameters.Geometry.CenterOnInitialize = false;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetPosition(state.NativeInstance, state.X, state.Y);
            }, (NativeInstance: _nativeInstance, value.X, value.Y));
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
                _startupParameters.Geometry.MaxWidth = value.X;
                _startupParameters.Geometry.MaxHeight = value.Y;
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
                _startupParameters.Geometry.MinWidth = value.X;
                _startupParameters.Geometry.MinHeight = value.Y;
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
                return _startupParameters.Geometry.Resizable;

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
                _startupParameters.Geometry.Resizable = value;
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
                return new Size(_startupParameters.Geometry.Width, _startupParameters.Geometry.Height);

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
                _startupParameters.Geometry.Width = value.Width;
                _startupParameters.Geometry.Height = value.Height;
                _startupParameters.Geometry.UseOsDefaultSize = false;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetSize(state.NativeInstance, state.Width, state.Height);
            }, (NativeInstance: _nativeInstance, value.Width, value.Height));
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
                _startupParameters.Window.Title = value;
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
                return _startupParameters.Geometry.Topmost;

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
                _startupParameters.Geometry.Topmost = value;
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
            return _startupParameters.Geometry.UseOsDefaultLocation;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Geometry.UseOsDefaultLocation = value;
            if (value)
                _startupParameters.Geometry.CenterOnInitialize = false;
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
            return _startupParameters.Geometry.UseOsDefaultSize;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Geometry.UseOsDefaultSize = value;
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
    /// Gets or sets the logging verbosity to standard output (Console/Terminal).
    /// 0 = Critical Only
    /// 1 = Critical and Warning
    /// 2 = Verbose
    /// >2 = All Details
    /// Default is 2.
    /// </summary>
    public int LogVerbosity { get; set; } = 2;

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
        Array.Clear(_startupParameters.Browser.CustomSchemeNames);
        var i = 0;
        foreach (var pair in CustomSchemes)
        {
            var scheme = pair.Key;
            if (!IsValidSchemeName(scheme))
                continue;
            _startupParameters.Browser.CustomSchemeNames[i++] = scheme;
            if (i == _startupParameters.Browser.CustomSchemeNames.Length)
                break;
        }

        _startupParameters.Window.Title = _title ?? DefaultTitle;
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
    /// Logs a message.
    /// </summary>
    /// <param name="message">Log message</param>
    internal void Log(string message)
    {
        if (LogVerbosity < 1) return;
        Console.WriteLine($"PhotinoX: \"{Title ?? DefaultTitle}\"{message}");
    }
}
