using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Photino.NET;

namespace WindowDiagnostics;

internal static class Program
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    [STAThread]
    private static int Main()
    {
        var app = new PhotinoApplication();

        var mainWindow = DiagnosticWindow.CreateMain();

        mainWindow.Window
            .SetTitle("PhotinoX Window Diagnostics")
            .SetUseOsDefaultSize(false)
            .SetSize(1200, 820)
            .Center()
            .Load("wwwroot/main.html");

        return app.Run(mainWindow.Window);
    }

    private sealed class DiagnosticWindow
    {
        private readonly List<LogEntry> _pendingEntries = [];
        private bool _clientReady;
        private bool _cancelClosing;
        private bool _logActions;
        private DiagnosticWindow? _child;

        private DiagnosticWindow(PhotinoWindow window, string id, string? parentId)
        {
            Window = window;
            Id = id;
            ParentId = parentId;
        }

        internal PhotinoWindow Window { get; }

        private string Id { get; }

        private string? ParentId { get; }

        internal static DiagnosticWindow CreateMain()
        {
            return Create(parent: null);
        }

        private static DiagnosticWindow Create(DiagnosticWindow? parent)
        {
            var id = parent is null
                ? "main"
                : $"{parent.Id}.child";

            var window = parent is null
                ? new PhotinoWindow()
                : new PhotinoWindow(parent.Window);

            var context = new DiagnosticWindow(window, id, parent?.Id);
            context.RegisterHandlers();

            return context;
        }

        private void RegisterHandlers()
        {
            Window
                .RegisterCreatingHandler((_, _) => Log("Creating"))
                .RegisterCreatedHandler((_, _) =>
                {
                    Log("Created");
                    SendState();
                })
                .RegisterClosingHandler((_, e) =>
                {
                    e.Cancel = _cancelClosing;
                    Log("Closing", new
                    {
                        e.Cancel
                    });
                })
                .RegisterClosedHandler((_, _) =>
                {
                    LogToConsole("Closed");

                    if (ParentId is not null)
                    {
                        LogToConsole($"Window '{Id}' closed.");
                    }
                })
                .RegisterLocationChangedHandler((_, location) =>
                {
                    Log("LocationChanged", new
                    {
                        location.X,
                        location.Y
                    });
                    SendState();
                })
                .RegisterSizeChangedHandler((_, size) =>
                {
                    Log("SizeChanged", new
                    {
                        size.Width,
                        size.Height
                    });
                    SendState();
                })
                .RegisterActivatedHandler((_, _) => Log("Activated"))
                .RegisterDeactivatedHandler((_, _) => Log("Deactivated"))
                .RegisterMaximizedHandler((_, _) =>
                {
                    Log("Maximized");
                    SendState();
                })
                .RegisterMinimizedHandler((_, _) =>
                {
                    Log("Minimized");
                    SendState();
                })
                .RegisterRestoredHandler((_, _) =>
                {
                    Log("Restored");
                    SendState();
                })
                .RegisterFullScreenEnteredHandler((_, _) =>
                {
                    Log("FullScreenEntered");
                    SendState();
                })
                .RegisterFullScreenExitedHandler((_, _) =>
                {
                    Log("FullScreenExited");
                    SendState();
                })
                .RegisterStateChangedHandler((_, state) =>
                {
                    Log("StateChanged", new
                    {
                        State = $"{state.OldState} -> {state.NewState}"
                    });

                    SendState();
                })
                .RegisterWebMessageReceivedHandler((_, message) =>
                {
                    HandleClientMessage(message);
                });

        }

        private void HandleClientMessage(string message)
        {
            ClientMessage? clientMessage;

            try
            {
                clientMessage = JsonSerializer.Deserialize<ClientMessage>(message, s_jsonOptions);
            }
            catch (Exception ex)
            {
                Log("InvalidClientMessage", new
                {
                    Error = ex.Message,
                    Message = message
                });
                return;
            }

            if (clientMessage is null)
                return;

            switch (clientMessage.Type)
            {
                case "ready":
                    Debug.Assert(Window.IsInitialized);
                    _clientReady = true;
                    SendInitialPayload();
                    FlushPendingEntries();
                    SendState();
                    break;

                case "action":
                    ExecuteAction(clientMessage.Target, clientMessage.Action);
                    break;

                case "setCancelClosing":
                    _cancelClosing = clientMessage.Value;
                    break;
                case "setLogActions":
                    _logActions = clientMessage.Value;
                    break;
            }
        }

        private void ExecuteAction(string? target, string? action)
        {
            if (string.IsNullOrWhiteSpace(action))
                return;

            var context = string.Equals(target, "child", StringComparison.OrdinalIgnoreCase)
                ? GetOrCreateChild(showIfCreated: false)
                : this;

            if (context is null)
                return;

            if (context._logActions)
            {
                context.LogDiagnostic("Action", new
                {
                    Target = target ?? "self",
                    Action = action
                });
            }

            try
            {
                switch (action)
                {
                    case "show":
                        GetOrCreateChild(showIfCreated: true);
                        break;

                    case "activate":
                        context.Window.Activate();
                        break;

                    case "bringToFront":
                        context.Window.BringToFront();
                        break;

                    case "maximize":
                        context.Window.Maximize();
                        break;

                    case "minimize":
                        context.Window.Minimize();
                        break;

                    case "restore":
                        context.Window.Restore();
                        break;

                    case "toggleFullScreen":
                        context.Window.SetFullScreen(context.Window.WindowState != PhotinoWindowState.FullScreen);
                        break;

                    case "close":
                        context.Window.Close();
                        break;

                    case "stateSequence":
                        context.RunStateSequence();
                        break;
                }
            }
            catch (Exception ex)
            {
                context.LogDiagnostic("ActionFailed", new
                {
                    Action = action,
                    ex.Message
                });
            }

            context.SendState();
        }

        private DiagnosticWindow? GetOrCreateChild(bool showIfCreated)
        {
            if (_child is { Window.IsClosed: false })
            {
                if (showIfCreated)
                    _child.Window.Show();

                return _child;
            }

            _child = Create(parent: this);

            _child.Window
                .SetTitle($"PhotinoX Window Diagnostics - Child of {Id}")
                .SetUseOsDefaultSize(false)
                .SetSize(1000, 720)
                .Center()
                .Load("wwwroot/main.html");

            _child.Window.RegisterClosedHandler((_, _) =>
            {
                Log("ChildClosed", new
                {
                    ChildId = _child?.Id
                });

                _child = null;
                SendState();
            });

            if (showIfCreated)
                _child.Window.Show();

            SendState();
            return _child;
        }

        private void RunStateSequence()
        {
            Window.Maximize();
            Window.Restore();
            Window.Minimize();
            Window.Restore();
            Window.SetFullScreen(true);
            Window.SetFullScreen(false);
        }

        private void SendInitialPayload()
        {
            Send(new
            {
                type = "init",
                id = Id,
                parentId = ParentId,
                title = Window.Title
            });
        }

        private void SendState()
        {
            if (!_clientReady || Window.IsClosed)
                return;

            try
            {
                var size = Window.IsInitialized ? Window.Size : Size.Empty;
                var location = Window.IsInitialized ? Window.Location : Point.Empty;

                Send(new
                {
                    type = "state",
                    id = Id,
                    parentId = ParentId,
                    isInitialized = Window.IsInitialized,
                    isClosed = Window.IsClosed,
                    hasChild = _child is { Window.IsClosed: false },
                    fullScreen = Window.WindowState == PhotinoWindowState.FullScreen,
                    state = Window.WindowState.ToString(),
                    size = new
                    {
                        size.Width,
                        size.Height
                    },
                    location = new
                    {
                        location.X,
                        location.Y
                    }
                });
            }
            catch (Exception ex)
            {
                LogToConsole($"Failed to send state: {ex}");
            }
        }

        private void Log(string eventName, object? payload = null)
        {
            var entry = new LogEntry(
                Type: "event",
                WindowId: Id,
                ParentId: ParentId,
                EventName: eventName,
                Timestamp: DateTimeOffset.Now,
                Payload: payload);

            LogToConsole(FormatConsoleLog(entry));

            if (!_clientReady || Window.IsClosed)
            {
                _pendingEntries.Add(entry);
                return;
            }

            Send(entry);
        }

        private void LogDiagnostic(string name, object? payload = null)
        {
            var entry = new LogEntry(
                Type: "diagnostic",
                WindowId: Id,
                ParentId: ParentId,
                EventName: name,
                Timestamp: DateTimeOffset.Now,
                Payload: payload);

            LogToConsole(FormatConsoleLog(entry));

            if (!_clientReady || Window.IsClosed)
            {
                _pendingEntries.Add(entry);
                return;
            }

            Send(entry);
        }

        private void FlushPendingEntries()
        {
            foreach (var entry in _pendingEntries)
                Send(entry);

            _pendingEntries.Clear();
        }

        private void Send(object value)
        {
            Debug.Assert(Window.IsInitialized);
            if (Window.IsClosed || !Window.IsInitialized)
                return;

            var json = JsonSerializer.Serialize(value, s_jsonOptions);
            Window.SendWebMessage(json);
        }

        private static string FormatConsoleLog(LogEntry entry)
        {
            var payload = entry.Payload is null
                ? string.Empty
                : $" | {JsonSerializer.Serialize(entry.Payload, s_jsonOptions)}";

            return $"{entry.Timestamp:HH:mm:ss.fff} | {entry.WindowId} | {entry.EventName}{payload}";
        }

        private static void LogToConsole(string message)
        {
            Console.WriteLine(message);
        }
    }

    private sealed record ClientMessage(
        string Type,
        string? Action,
        string? Target,
        bool Value);

    private sealed record LogEntry(
        string Type,
        string WindowId,
        string? ParentId,
        string EventName,
        DateTimeOffset Timestamp,
        object? Payload);
}