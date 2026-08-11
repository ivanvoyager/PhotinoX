using System.Text.Json;
using System.Text.Json.Serialization;

using Photino.NET;

PhotinoWindow? window = null;
var webReady = false;
var pendingMessages = new Queue<string>();

var application = new PhotinoApplication()
    .SetName("NotificationDiagnostics")
    .SetIconPath(Path.Combine(AppContext.BaseDirectory, "assets", "logo.png"))
    .SetNotificationRegistrationId("PhotinoX.NotificationDiagnostics")
    .SetNotificationsEnabled(true)
    .RegisterStartupHandler((_, _) => AddLog("Application.Startup"))
    .RegisterExitHandler((_, e) => Console.WriteLine($"Application.Exit: {e.ApplicationExitCode}"))
    .RegisterNotificationActionActivatedHandler((_, e) =>
        AddLog($"NotificationActionActivated: NotificationId={e.NotificationId}, ActionIndex={e.ActionIndex}"))
    .RegisterNotificationInputActivatedHandler((_, e) =>
        AddLog($"NotificationInputActivated: NotificationId={e.NotificationId}, Response={e.Response}"))
    .RegisterNotificationActivatedHandler((_, e) => AddLog($"NotificationActivated: {e.NotificationId}"))
    .RegisterNotificationDismissedHandler((_, e) => AddLog($"NotificationDismissed: {e.NotificationId}, {e.Reason}"))
    .RegisterNotificationFailedHandler((_, e) => AddLog($"NotificationFailed: {e.NotificationId}"))
    .RegisterDispatcherUnhandledExceptionHandler((_, e) => Console.WriteLine($"DispatcherUnhandledException: {e.ExceptionObject}"));

window = new PhotinoWindow()
    .SetTitle("Notification Diagnostics")
    .SetSize(980, 720)
    .Center()
    .RegisterWebMessageReceivedHandler((_, e) =>
    {
        HandleWebMessage(e.Message);
    })
    .Load("wwwroot/index.html");

return application.Run(window);

void HandleWebMessage(string message)
{
    using var document = JsonDocument.Parse(message);
    var root = document.RootElement;

    var command = root.GetProperty("command").GetString();

    switch (command)
    {
        case "ready":
            webReady = true;
            FlushPendingMessages();
            AddLog("Web UI ready");
            SendState();
            break;

        case "show":
            ShowNotification(root);
            break;

        case "setNotificationsEnabled":
            SetNotificationsEnabled(root);
            break;

        case "shutdown":
            application.Shutdown();
            break;
    }
}

void ShowNotification(JsonElement root)
{
    var title = root.GetProperty("title").GetString() ?? string.Empty;
    var body = root.GetProperty("body").GetString() ?? string.Empty;
    var iconPath = root.TryGetProperty("iconPath", out var iconElement)
        ? iconElement.GetString()
        : null;

    if (string.IsNullOrWhiteSpace(iconPath))
        iconPath = null;

    try
    {
        var notificationId = application.ShowNotification(title, body, iconPath);
        AddLog($"ShowNotification: NotificationId={notificationId}, Title=\"{title}\", Body=\"{body}\", IconPath=\"{iconPath ?? ""}\"");
    }
    catch (Exception ex)
    {
        AddLog($"ShowNotification failed: {ex.Message}");
    }
}

void SetNotificationsEnabled(JsonElement root)
{
    var enabled = root.GetProperty("enabled").GetBoolean();

    try
    {
        application.NotificationsEnabled = enabled;
        AddLog($"NotificationsEnabled changed: {enabled}");
        SendState();
    }
    catch (Exception ex)
    {
        AddLog($"NotificationsEnabled change failed: {ex.Message}");
        SendState();
    }
}

void SendState()
{
    SendToWeb(JsonSerializer.Serialize(
        new StatePayload(
            Type: "state",
            ApplicationName: application.Name,
            NotificationRegistrationId: application.NotificationRegistrationId,
            NotificationsEnabled: application.NotificationsEnabled,
            IsRunning: application.IsRunning),
        NotificationDiagnosticsJsonContext.Default.StatePayload));
}

void AddLog(string message)
{
    SendToWeb(JsonSerializer.Serialize(
        new LogPayload(
            Type: "log",
            Timestamp: DateTimeOffset.Now.ToString("HH:mm:ss.fff"),
            Message: message),
        NotificationDiagnosticsJsonContext.Default.LogPayload));
}

void SendToWeb(string json)
{
    if (!webReady || window is null)
    {
        pendingMessages.Enqueue(json);
        return;
    }

    try
    {
        window.SendWebMessage(json);
    }
    catch (InvalidOperationException)
    {
        webReady = false;
        pendingMessages.Clear();
    }
}

void FlushPendingMessages()
{
    if (window is null)
        return;

    try
    {
        while (pendingMessages.Count > 0)
        {
            window.SendWebMessage(pendingMessages.Dequeue());
        }
    }
    catch (InvalidOperationException)
    {
        webReady = false;
        pendingMessages.Clear();
    }
}

internal readonly record struct LogPayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("message")] string Message);

internal readonly record struct StatePayload(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("applicationName")] string? ApplicationName,
    [property: JsonPropertyName("notificationRegistrationId")] string? NotificationRegistrationId,
    [property: JsonPropertyName("notificationsEnabled")] bool NotificationsEnabled,
    [property: JsonPropertyName("isRunning")] bool IsRunning);

[JsonSerializable(typeof(LogPayload))]
[JsonSerializable(typeof(StatePayload))]
internal partial class NotificationDiagnosticsJsonContext : JsonSerializerContext;