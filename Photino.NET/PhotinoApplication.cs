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
public sealed class PhotinoApplication
{
    private static int s_appCreated;
    private int _isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhotinoApplication"/> class.
    /// </summary>
    public PhotinoApplication()
    {
        if (Interlocked.CompareExchange(ref s_appCreated, 1, 0) == 1)
        {
            throw new InvalidOperationException($"Cannot create more than one {typeof(PhotinoApplication).FullName} instance.");
        }

        Current = this;
    }

    /// <summary>
    /// Gets the current application instance.
    /// </summary>
    /// <remarks>
    /// The current application is assigned when an application instance is created.
    /// </remarks>
    public static PhotinoApplication Current
    {
        get => field ?? throw new InvalidOperationException("PhotinoApplication has not been created.");
        private set;
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
    /// <remarks>
    /// Available after the application message loop has been started.
    /// </remarks>
    public PhotinoDispatcher Dispatcher
    {
        get => field ?? throw new InvalidOperationException("The application is not running.");
        private set;
    }

    /// <summary>
    /// Gets a value indicating whether the application is running.
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
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1) { throw new InvalidOperationException("The application is already running."); }

        if (Platform.IsWindows && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            return RunOnStaThread(mainWindow);
        }

        return RunCore(mainWindow);
    }

    private int RunOnStaThread(PhotinoWindow? mainWindow)
    {
        if (mainWindow is not null && mainWindow.IsNativeCreated)
        {
            throw new InvalidOperationException("A window created on a non-STA thread cannot be moved to an STA application thread.");
        }

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
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        return exitCode;
    }

    private int RunCore(PhotinoWindow? mainWindow)
    {
        try
        {
            return RunNative();
        }
        finally
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
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
}
