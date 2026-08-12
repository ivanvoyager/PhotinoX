using System.Drawing;
using System.Text;
using Photino.NET;

namespace CustomSchemes;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var app = new PhotinoApplication();

        var window = new PhotinoWindow()
            .SetTitle("PhotinoX Custom Schemes")
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(900, 700))
            .Center()
            .SetDevToolsEnabled(true)
            .RegisterCustomSchemeHandler("app", HandleAppScheme)
            .RegisterWebMessageReceivedHandler((sender, message) =>
            {
                Console.WriteLine($"Message from webview: {message}");
            })
            .Load(new Uri("app://localhost/index.html"));

        app.Run(window);
    }

    private static Stream HandleAppScheme(
        object? sender,
        string scheme,
        string url,
        out string? contentType)
    {
        Console.WriteLine($"Custom scheme request: {url}");

        Uri uri = new(url);
        string path = string.IsNullOrEmpty(uri.AbsolutePath) ? "/index.html" : uri.AbsolutePath;

        string basePath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        string filePath = path switch
        {
            "/" or "/index.html" => Path.Combine(basePath, "index.html"),
            "/style.css" => Path.Combine(basePath, "style.css"),
            "/app.js" => Path.Combine(basePath, "app.js"),
            "/data.json" => null!,
            _ => null!,
        };

        if (path == "/data.json")
        {
            contentType = "application/json";
            return new MemoryStream(Encoding.UTF8.GetBytes("""
            {
                "status": "ok",
                "source": "app://localhost/data.json"
            }
            """));
        }

        if (filePath is null || !File.Exists(filePath))
        {
            contentType = "text/plain";
            return new MemoryStream(Encoding.UTF8.GetBytes($"Not found: {url}"));
        }

        contentType = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "text/javascript",
            ".json" => "application/json",
            _ => "application/octet-stream",
        };

        return File.OpenRead(filePath);
    }
}