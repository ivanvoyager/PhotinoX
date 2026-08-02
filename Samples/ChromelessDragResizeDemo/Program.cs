using System.Drawing;
using Photino.NET;

namespace ChromelessDragResizeDemo;

// Standalone top-level chromeless window whose title bar and resize grips are drawn
// in HTML and driven by BeginWindowDrag / BeginWindowResize (wwwroot/chromeless.html).

internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var app = new PhotinoApplication();

        var mainWindow = new PhotinoWindow()
            .SetTitle("Chromeless Demo")
            .SetChromeless(true)
            .SetLinuxChromelessDragRegion(36, 110)
            .SetResizable(true)
            .SetUseOsDefaultLocation(false)
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(520, 360))
            .SetMinSize(320, 200)
            .Center()
            .Load("wwwroot/chromeless.html")
            .RegisterWebMessageReceivedHandler(WindowWebMessageReceived)
            .RegisterLocationChangedHandler(WindowLocationChanged)
            .RegisterSizeChangedHandler(WindowSizeChanged)
            .RegisterStateChangedHandler(WindowStateChanged)
            .RegisterContentLoadedHandler(WindowContentLoaded);

        app.Run(mainWindow);
    }

    private static void WindowWebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
    {
        Log(sender, $"WebMessageReceived Callback Fired.");

        if (sender is not PhotinoWindow currentWindow) return;

        var message = e.Message;

        if (string.Equals(message, "begindrag", StringComparison.OrdinalIgnoreCase))
        {
            currentWindow.BeginWindowDrag();
        }
        else if (message.StartsWith("beginresize-", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<PhotinoWindowEdge>(message["beginresize-".Length..], ignoreCase: true, out var edge))
                currentWindow.BeginWindowResize(edge);
        }
        else if (string.Equals(message, "minimize", StringComparison.OrdinalIgnoreCase))
        {
            currentWindow.Minimize();
        }
        else if (string.Equals(message, "maximize", StringComparison.OrdinalIgnoreCase))
        {
            if (currentWindow.WindowState == PhotinoWindowState.Maximized)
                currentWindow.Restore();
            else
                currentWindow.Maximize();
        }
        else if (string.Equals(message, "close", StringComparison.OrdinalIgnoreCase))
        {
            currentWindow.Close();
        }
        else
        {
            Log(sender, $"Unknown message '{message}'");
        }
    }

    private static void WindowLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        Log(sender, $"LocationChanged Callback Fired.  Left: {e.Left}  Top: {e.Top}");
    }
    private static void WindowSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        Log(sender, $"SizeChanged Callback Fired.  Height: {e.Height}  Width: {e.Width}");
    }

    private static void WindowStateChanged(object? sender, StateChangedEventArgs e)
    {
        Log(sender, $"StateChanged Callback Fired.  Old: {e.OldState}  New: {e.NewState}");
    }

    private static void WindowContentLoaded(object? sender, ContentLoadedEventArgs e)
    {
        Log(sender, $"ContentLoaded Callback Fired.  URI: {e.Uri}");
    }

    private static void Log(object? sender, string message)
    {
        var windowTitle = sender is PhotinoWindow currentWindow ? currentWindow.Title : string.Empty;
        Console.WriteLine($"-Client App: \"{windowTitle}\" {message}");
    }
}
