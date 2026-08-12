using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

using static Photino.NET.NativeMethods;

namespace Photino.NET;
/// <summary>
/// Represents a Photino application lifetime object.
/// </summary>
/// <remarks>
/// A <see cref="PhotinoApplication"/> owns the application-level lifetime, window tracking,
/// dispatcher access, shutdown behavior, and message-loop execution.
/// </remarks>
public sealed partial class PhotinoApplication
{
    private PhotinoApplicationNativeParameters _startupParameters = new()
    {
        Size = Marshal.SizeOf<PhotinoApplicationNativeParameters>(),
        AbiVersion = PhotinoApplicationNativeParameters.NativeAbiVersion,

        NotificationsEnabled = true
    };

    private static PhotinoApplication? s_current;
    private int _isRunning;
    private int _isInMainLoop;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotinoApplication"/> class.
    /// </summary>
    public PhotinoApplication() : this(registerCurrent: true)
    {
    }

    private PhotinoApplication(bool registerCurrent)
    {
        if (registerCurrent && Volatile.Read(ref s_current) is not null)
        {
            ThrowApplicationAlreadyCreated();
        }

        PhotinoBootstrap.Initialize();
        Dispatcher = new PhotinoDispatcher();

        if (registerCurrent && Interlocked.CompareExchange(ref s_current, this, null) is not null)
        {
            ThrowApplicationAlreadyCreated();
        }

        _startupParameters.ApplicationName =
            _startupParameters.NotificationRegistrationId = GetDefaultApplicationName();

        _startupParameters.StartupHandler = OnStartup;
        _startupParameters.ShutdownRequestedHandler = OnShutdownRequested;
        _startupParameters.ExitHandler = OnExit;

        _startupParameters.NotificationActivatedHandler = OnNotificationActivated;
        _startupParameters.NotificationActionActivatedHandler = OnNotificationActionActivated;
        _startupParameters.NotificationInputActivatedHandler = OnNotificationInputActivated;
        _startupParameters.NotificationDismissedHandler = OnNotificationDismissed;
        _startupParameters.NotificationFailedHandler = OnNotificationFailed;
    }

    /// <summary>
    /// Gets or sets the application display name used by application-level native services.
    /// </summary>
    /// <remarks>
    /// This value is passed to the native application layer when <see cref="Run(PhotinoWindow?)"/> starts.
    /// It cannot be changed after the application has started.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when setting the value after the application has started.
    /// </exception>
    public string? Name
    {
        get => _startupParameters.ApplicationName;
        set
        {
            ThrowIfRunning();

            _startupParameters.ApplicationName = value;
        }
    }

    /// <summary>
    /// Gets or sets the application icon path used by application-level native services.
    /// </summary>
    /// <remarks>
    /// This value is passed to the native application layer when <see cref="Run(PhotinoWindow?)"/> starts.
    /// It cannot be changed after the application has started.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when setting the value after the application has started.
    /// </exception>
    public string? IconPath
    {
        get => _startupParameters.ApplicationIconPath;
        set
        {
            ThrowIfRunning();

            _startupParameters.ApplicationIconPath = value;
        }
    }

    /// <summary>
    /// Occurs when an exception thrown by an asynchronous dispatcher callback is not handled by the callback path.
    /// </summary>
    /// <remarks>
    /// This event forwards dispatcher-level unhandled exception notifications.
    /// The supplied <see cref="UnhandledExceptionEventArgs"/> does not provide a handled flag.
    /// </remarks>
    public event UnhandledExceptionEventHandler? DispatcherUnhandledException
    {
        add => Dispatcher.UnhandledException += value;
        remove => Dispatcher.UnhandledException -= value;
    }

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    /// <remarks>
    /// The current application is created on first access if no application instance has been created explicitly.
    /// </remarks>
    public static PhotinoApplication Current
    {
        get
        {
            var current = Volatile.Read(ref s_current);
            if (current is not null)
                return current;

            var application = new PhotinoApplication(registerCurrent: false);
            current = Interlocked.CompareExchange(ref s_current, application, null);

            return current ?? application;
        }
    }

    /// <summary>
    /// Gets the main application window.
    /// </summary>
    /// <remarks>
    /// The main window is assigned when <see cref="Run(PhotinoWindow?)"/> is called with a window.
    /// The value may be <c>null</c> when the application is started without a main window.
    /// </remarks>
    public PhotinoWindow? MainWindow { get; private set; }

    /// <summary>
    /// Gets the windows currently owned by the application.
    /// </summary>
    public PhotinoWindowCollection Windows { get; } = [];

    /// <summary>
    /// Gets the dispatcher associated with the application UI thread.
    /// </summary>
    public PhotinoDispatcher Dispatcher { get; }

    /// <summary>
    /// Gets a value indicating whether the application is running or starting.
    /// </summary>
    public bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    /// <summary>
    /// Gets or sets the shutdown mode for the application.
    /// </summary>
    /// <remarks>
    /// The shutdown mode controls whether the application exits when the main window closes,
    /// when the last window closes, or only when <see cref="Shutdown(int, bool)"/> is called explicitly.
    /// </remarks>
    /// <exception cref="InvalidEnumArgumentException">
    /// Thrown when setting an undefined <see cref="PhotinoShutdownMode"/> value.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when setting the value while the application is shutting down.
    /// </exception>
    public PhotinoShutdownMode ShutdownMode
    {
        get;
        set
        {
            ThrowIfInvalidShutdownMode(value);
            ThrowIfShuttingDown();
            field = value;
        }
    } = PhotinoShutdownMode.OnLastWindowClose;

    private static bool NativeIsRunning => PhotinoApplication_IsRunning();

    private static bool NativeIsShuttingDown => PhotinoApplication_IsShuttingDown();

    /// <summary>
    /// Gets a value indicating whether the native application is shutting down.
    /// </summary>
    public bool IsShuttingDown => IsRunning && NativeIsShuttingDown;

    /// <summary>
    /// Runs the application message loop.
    /// </summary>
    /// <param name="mainWindow">
    /// The main window to show and run with the application. If <c>null</c>, the application
    /// runs without assigning a main window.
    /// </param>
    /// <returns>
    /// The application exit code.
    /// </returns>
    /// <remarks>
    /// When <paramref name="mainWindow"/> is provided, it becomes the <see cref="MainWindow"/>.
    /// The application continues running until its shutdown conditions are met or
    /// <see cref="Shutdown(int, bool)"/> is called.
    /// </remarks>
    public int Run(PhotinoWindow? mainWindow = null)
    {
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
            ThrowApplicationAlreadyRunning();

        try
        {
            if (Platform.IsWindows && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                return RunOnStaThread(mainWindow);
            }

            return RunCore(mainWindow);
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }

    private int RunOnStaThread(PhotinoWindow? mainWindow)
    {
        if (mainWindow is not null && mainWindow.IsInitialized)
            ThrowNativeWindowCannotBeMovedToStaThread();

        var exitCode = 0;
        Exception? exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                exitCode = RunCore(mainWindow);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
#pragma warning disable CA1416 // Only for Windows
        thread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416 // Only for Windows
        thread.IsBackground = false;
        thread.Start();
        thread.Join();

        if (exception is not null)
            ExceptionDispatchInfo.Capture(exception).Throw();

        return exitCode;
    }

    private int RunCore(PhotinoWindow? mainWindow)
    {
        Dispatcher.VerifyAccessToCreateWindow();

        if (mainWindow is not null)
        {
            MainWindow = mainWindow;
            try
            {
                mainWindow.Show();
            }
            catch
            {
                MainWindow = null;
                throw;
            }
        }

        Debug.Assert(_startupParameters.Size == 104);
        Volatile.Write(ref _isInMainLoop, 1);
        try
        {
            return PhotinoApplication_Run(ref _startupParameters);
        }
        finally
        {
            Volatile.Write(ref _isInMainLoop, 0);
            ClearNotificationStates();
        }
    }

    /// <summary>
    /// Requests application shutdown.
    /// </summary>
    /// <param name="exitCode">
    /// The exit code returned by <see cref="Run(PhotinoWindow?)"/>.
    /// </param>
    /// <param name="force">
    /// <see langword="true"/> to bypass <see cref="ShutdownRequested"/>; otherwise, <see langword="false"/>
    /// to allow the shutdown request to be canceled.
    /// </param>
    /// <remarks>
    /// Unless <paramref name="force"/> is <see langword="true"/>, the request can be canceled by
    /// <see cref="ShutdownRequested"/> handlers.
    /// </remarks>
    public void Shutdown(int exitCode = 0, bool force = false)
    {
        _ = this;
        PhotinoApplication_Shutdown(exitCode, force ? (byte)1 : (byte)0);
    }

    internal void OnWindowCreated(PhotinoWindow window)
    {
        Windows.Add(window);
    }

    internal void OnWindowClosed(PhotinoWindow window)
    {
        Windows.Remove(window);

        bool isMainWindow = ReferenceEquals(window, MainWindow);
        if (isMainWindow)
            MainWindow = null;

        if (ShutdownMode == PhotinoShutdownMode.OnExplicitShutdown)
            return;

        if (ShutdownMode == PhotinoShutdownMode.OnMainWindowClose && isMainWindow)
        {
            CloseWindows();
            Shutdown(force: true);
            return;
        }

        if (ShutdownMode == PhotinoShutdownMode.OnLastWindowClose && Windows.Count == 0)
            Shutdown(force: true);
    }

    private void CloseWindows()
    {
        var windowsToClose = Windows.ToArray();
        for (int i = windowsToClose.Length - 1; i >= 0; i--)
        {
            windowsToClose[i].InternalClose();
        }
    }

    private static string GetDefaultApplicationName()
    {
        var name = Assembly.GetEntryAssembly()?.GetName().Name;
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        name = Process.GetCurrentProcess().ProcessName;
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        return "PhotinoX";
    }
}
