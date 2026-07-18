using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Photino.NET.Utils;

namespace Photino.NET;

using static NativeMethods;

/// <summary>
/// Represents a Photino application lifetime object.
/// </summary>
/// <remarks>
/// A <see cref="PhotinoApplication"/> owns the application-level lifetime, window tracking,
/// dispatcher access, shutdown behavior, and message-loop execution.
/// </remarks>
public sealed partial class PhotinoApplication
{
    private static PhotinoApplication? s_current;
    private int _isRunning;

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
    /// when the last window closes, or only when <see cref="Shutdown(int)"/> is called explicitly.
    /// </remarks>
    public PhotinoShutdownMode ShutdownMode { get; set; } = PhotinoShutdownMode.OnLastWindowClose;

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
    /// <see cref="Shutdown(int)"/> is called.
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
        if (mainWindow is not null && mainWindow.IsNativeCreated)
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

        return PhotinoApplication_Run();
    }

    /// <summary>
    /// Requests application shutdown.
    /// </summary>
    /// <param name="exitCode">
    /// The exit code returned by <see cref="Run(PhotinoWindow?)"/>.
    /// </param>
    public void Shutdown(int exitCode = 0)
    {
        PhotinoApplication_Shutdown(exitCode);
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
            Shutdown();
            return;
        }

        if (ShutdownMode == PhotinoShutdownMode.OnLastWindowClose && Windows.Count == 0)
            Shutdown();
    }

    private void CloseWindows()
    {
        var windowsToClose = Windows.ToArray();
        for (int i = windowsToClose.Length - 1; i >= 0; i--)
        {
            windowsToClose[i].InternalClose();
        }
    }

    private static void ThrowApplicationAlreadyCreated()
    {
        throw new InvalidOperationException($"Cannot create more than one {typeof(PhotinoApplication).FullName} instance.");
    }

    private static void ThrowApplicationAlreadyRunning()
    {
        throw new InvalidOperationException("The application is already running.");
    }

    private static void ThrowNativeWindowCannotBeMovedToStaThread()
    {
        throw new InvalidOperationException("An initialized native window cannot be moved to another application thread.");
    }
}
