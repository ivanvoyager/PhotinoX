using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Photino.NET;

var app = new PhotinoApplication
{
    ShutdownMode = PhotinoShutdownMode.OnMainWindowClose
};

var splashWindow = new PhotinoWindow()
    .SetTitle("PhotinoX")
    .SetChromeless(true)
    .SetResizable(false)
    .SetSize(480, 300)
    .Center()
    .Load("wwwroot/splash.html");

splashWindow.RegisterInitialContentLoadedHandler((_, __) =>
{
    _ = ShowSplashAsync();
});

async Task ShowSplashAsync()
{
    await Task.Delay(100); // Allow the WebView surface to be presented before showing the native window.

    app.Dispatcher.BeginInvoke(() =>
    {
        splashWindow.Show();
        _ = StartApplicationAsync();
    });
}

app.RegisterStartupHandler((_, _) =>
{
    splashWindow.Initialize();
});

WebApplication? webApplication = null;

var exitCode = app.Run();

if (webApplication is not null)
{
    webApplication.StopAsync().GetAwaiter().GetResult();
    webApplication.DisposeAsync().AsTask().GetAwaiter().GetResult();
}

return exitCode;

async Task StartApplicationAsync()
{
    try
    {
        await Task.Delay(1500); // Simulate application startup work.
        webApplication = await StartWebApplicationAsync(args);

        var addressFeature = webApplication.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>();

        string? address = addressFeature?.Addresses.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(address))
            throw new InvalidOperationException("Kestrel did not publish an application address.");

        await app.Dispatcher.InvokeAsync(() =>
        {
            var mainWindow = new PhotinoWindow()
                .SetTitle("PhotinoX Splash Screen Sample")
                .SetSize(1200, 820)
                .Center()
                .Load(address);

            app.MainWindow = mainWindow;
            mainWindow.Show();

            splashWindow.Close();
        });
    }
    catch (Exception ex)
    {
        Debug.Fail($"Application startup failed: {ex}");
        Console.Error.WriteLine(ex);

        app.Dispatcher.BeginInvoke(() =>
        {
            if (!splashWindow.IsClosed)
                splashWindow.Close();

            app.Shutdown(1, force: true);
        });
    }
}

async Task<WebApplication> StartWebApplicationAsync(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(IPAddress.Loopback, 0);
    });

    var application = builder.Build();

    application.UseDefaultFiles();
    application.UseStaticFiles();

    await application.StartAsync();

    return application;
}
