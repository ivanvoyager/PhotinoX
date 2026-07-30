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
            .RegisterStateChangedHandler(WindowStateChanged);

        app.Run(mainWindow);
    }

    private static void WindowWebMessageReceived(object? sender, string message)
    {
        Log(sender, $"WindowWebMessageReceived Callback Fired.");

        if (sender is not PhotinoWindow currentWindow) return;

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

    private static void WindowLocationChanged(object? sender, Point location)
    {
        Log(sender, $"WindowLocationChanged Callback Fired.  Left: {location.X}  Top: {location.Y}");
    }
    private static void WindowSizeChanged(object? sender, Size size)
    {
        Log(sender, $"WindowSizeChanged Callback Fired.  Height: {size.Height}  Width: {size.Width}");
    }

    private static void WindowStateChanged(object? sender, PhotinoWindowStateChangedEventArgs e)
    {
        Log(sender, $"WindowStateChanged Callback Fired.  Old: {e.OldState}  New: {e.NewState}");
    }
    
    private static void Log(object? sender, string message)
    {
        var windowTitle = sender is PhotinoWindow currentWindow ? currentWindow.Title : string.Empty;
        Console.WriteLine($"-Client App: \"{windowTitle}\" {message}");
    }
}
