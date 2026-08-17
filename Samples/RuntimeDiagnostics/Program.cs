using System.Text.Json;
using Photino.NET;

var app = new PhotinoApplication();

var window = new PhotinoWindow()
    .SetTitle("PhotinoX Runtime Diagnostics")
    .SetWidth(700)
    .SetHeight(560)
    .Center()
    .Load(Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html"));

window.WebMessageReceived += (_, args) =>
{
    if (!string.Equals(args.Message, "getRuntimeInfo", StringComparison.Ordinal))
        return;

    var runtimeInfo = app.GetRuntimeInfo();

    var json = JsonSerializer.Serialize(runtimeInfo, new JsonSerializerOptions
    {
        WriteIndented = true
    });

    window.SendWebMessage(json);
};

app.Run(window);