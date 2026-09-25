using System.Diagnostics;
using Photino.NET;

var application = new PhotinoApplication();

var window = new PhotinoWindow();

window.SetTitle("PhotinoX HelloWorld")
    .Center()
    .LoadString("""
    <!DOCTYPE html>
    <html>
        <head>
            <meta charset="utf-8">
            <title>PhotinoX Web Messaging</title>
        </head>
        <body>
            <h1>Web messaging</h1>
            <button id="send-message">Send message to .NET</button>

            <script>
                document
                    .getElementById("send-message")
                    .addEventListener("click", () =>
                    {
                        window.external.sendMessage("Hello from JavaScript");
                    });
            </script>
        </body>
    </html>
    """)
    .RegisterCreatingHandler((_, __) =>
    {
        Console.WriteLine("Creating window");
    })
    .RegisterCreatedHandler((_, __) =>
    {
        Console.WriteLine("Created window, count: " + application.Windows.Count);

        var windows = application.Windows.ToList();

        Debug.Assert(windows.Count == 1);
        Debug.Assert(windows[0] == window);
        Debug.Assert(windows.Contains(window));
        Debug.Assert(windows.Count == 1);
        Debug.Assert(windows.Any());
    })
    .RegisterClosingHandler((_, args) =>
    {
        Console.WriteLine("Closing window");
        args.Cancel = false;
    })
    .RegisterClosedHandler((_, __) =>
    {
        Console.WriteLine("Closed window, count: " + application.Windows.Count);
        Debug.Assert(!application.Windows.Any());
    })
    .RegisterWebMessageReceivedHandler((_, args) =>
    {
        Console.WriteLine("Message: " + args.Message);
        Console.WriteLine("Source: " + args.Uri);
    });

application
    .SetName("PhotinoX HelloWorld")
    .SetShutdownMode(PhotinoShutdownMode.OnMainWindowClose)
    .SetNotificationsEnabled(false)
    .RegisterStartupHandler((_, __) =>
    {
        Console.WriteLine("Startup handler: " + application.Name);
        Console.WriteLine("Notifications enabled: " + application.NotificationsEnabled);

        application.SetNotificationsEnabled(true);

        Console.WriteLine("Notifications enabled: " + application.NotificationsEnabled);
    })
    .RegisterExitHandler((_, args) =>
    {
        Console.WriteLine("Exit handler: " + args.ApplicationExitCode);
    });

return application.Run(window);