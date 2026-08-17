using System.Diagnostics;
using System.Drawing;
using Photino.NET;

namespace NavigationPolicyDemo;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var app = new PhotinoApplication();

        var window = new PhotinoWindow()
            .SetTitle("Navigation Policy Demo")
            .SetUseOsDefaultSize(false)
            .SetUseOsDefaultLocation(false)
            .SetSize(new Size(960, 700))
            .Center()
            .SetStatusBarEnabled(false)
            .Load("wwwroot/navigation-policy.html")
            .RegisterContentLoadedHandler(WindowContentLoaded)
            .RegisterWebMessageReceivedHandler(WindowWebMessageReceived);

        ConfigureNavigationPolicy(window);

        return app.Run(window);
    }

    private static void ConfigureNavigationPolicy(PhotinoWindow window)
    {
        window.RegisterNavigationStartingHandler((_, e) =>
        {
            Log(window, $"NavigationStarting: {e.Uri}");

            if (IsExternalUri(e.Uri))
            {
                e.Cancel = true;
                OpenExternalBrowser(e.Uri);
            }
        });

        window.RegisterNewWindowRequestedHandler((_, e) =>
        {
            Log(window, $"NewWindowRequested: {e.Uri}");

            OpenExternalBrowser(e.Uri);
        });
    }

    private static void WindowContentLoaded(object? sender, ContentLoadedEventArgs e)
    {
        Log(sender, $"ContentLoaded: {e.Uri}");
    }

    private static void WindowWebMessageReceived(object? sender, WebMessageReceivedEventArgs e)
    {
        Log(sender, $"WebMessageReceived: {e.Message}  Uri: {e.Uri}");
    }

    private static bool IsExternalUri(Uri uri)
    {
        return uri.Scheme == Uri.UriSchemeHttp ||
             uri.Scheme == Uri.UriSchemeHttps;
    }

    private static void OpenExternalBrowser(Uri uri)
    {
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
        {
            UseShellExecute = true
        });
    }

    private static void Log(object? sender, string message)
    {
        var title = sender is PhotinoWindow window ? window.Title : string.Empty;
        Console.WriteLine($"-Client App: \"{title}\" {message}");
    }
}