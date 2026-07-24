[![PhotinoX Logo](https://raw.githubusercontent.com/ivanvoyager/PhotinoX/refs/heads/master/assets/photinox-logo.png)](https://github.com/ivanvoyager/PhotinoX)

# PhotinoX

[![NuGet Version](https://img.shields.io/nuget/v/PhotinoX.svg)](https://www.nuget.org/packages/PhotinoX)
[![Build](https://github.com/ivanvoyager/PhotinoX/actions/workflows/build.yml/badge.svg)](https://github.com/ivanvoyager/PhotinoX/actions/workflows/build.yml)
[![License](https://img.shields.io/github/license/ivanvoyager/PhotinoX?label=license)](https://github.com/ivanvoyager/PhotinoX/blob/master/LICENSE)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PhotinoX.svg)](https://www.nuget.org/packages/PhotinoX)

Lightweight **.NET wrapper** for native OS WebView windows:
- **Windows**: WebView2
- **macOS**: WKWebView
- **Linux**: WebKitGTK 4.1

PhotinoX is a maintained fork of Photino.NET focused on predictable cross-platform desktop behavior, native runtime stability, and a cleaner managed API surface.

## What is PhotinoX?

PhotinoX builds on the original Photino design: native desktop windows hosted by modern **Web UI technologies** (Blazor, React, Vue, Angular, etc.), without bundling a full Chromium runtime.  
It relies entirely on **OS‑native WebView engines**, keeping apps small and efficient.

> **Note:** PhotinoX is an independent fork of [tryphotino/photino.NET](https://github.com/tryphotino/photino.NET) under the Apache‑2.0 license and is **not affiliated** with the original project or organization.

## How PhotinoX differs from Photino.NET

PhotinoX differs from the original Photino.NET project in several managed API areas: an explicit `PhotinoApplication` model, UI-thread dispatching through `PhotinoDispatcher`, simplified window event names, clearer window state operations, and a cleaner fluent `PhotinoWindow` API.

### Application model

`PhotinoApplication` is the explicit application lifetime object. Window creation, shutdown behavior, and UI-thread dispatching are coordinated through the application and its dispatcher instead of implicit global state. The application also tracks open windows and exposes `MainWindow` and `Windows`.

| Previous model | New model |
|---|---|
| `PhotinoWindow.WaitForClose()` creates the native window and starts the message loop. | `PhotinoApplication.Run(window)` owns application lifetime and message-loop execution. |
| Window creation and message-loop state are controlled from `PhotinoWindow`. | `PhotinoApplication.Run(window)` shows the main window; explicit window creation/showing is available through `PhotinoWindow.Show()`. |
| Window lifetime is centered around individual `PhotinoWindow` instances. | `PhotinoApplication` tracks open windows through `MainWindow` and `Windows`. |
| `PhotinoWindow.Invoke(...)` dispatches through native window-level invoke helpers. | UI-thread dispatching is centralized through `PhotinoApplication.Current.Dispatcher`, including `CheckAccess`, `Invoke`, `TryInvoke`, `BeginInvoke`, and `InvokeAsync`. |
| Shutdown behavior is implicit around the native message loop. | Shutdown behavior is controlled by `PhotinoShutdownMode` and `PhotinoApplication.Shutdown(...)`. |

```csharp
var app = new PhotinoApplication();

var window = new PhotinoWindow()
    .SetTitle("PhotinoX")
    .Load("index.html")
    .RegisterClosingHandler((_, e) => e.Cancel = true);

return app.Run(window);
```

`PhotinoDispatcher` provides application-level UI-thread dispatching. It supports synchronous dispatch through `Invoke(...)` / `TryInvoke(...)`, asynchronous fire-and-forget dispatch through `BeginInvoke(...)`, task-based dispatch through `InvokeAsync(...)`, dispatcher access checks through `CheckAccess()` / `VerifyAccess()`, and `UnhandledException` notifications for exceptions raised by asynchronous dispatcher callbacks.

### Window events

Window event names are simplified to remove redundant `Window` prefixes and align better with common .NET event naming. Closing now uses standard `CancelEventArgs`, focus events are exposed as `Activated` and `Deactivated`, and window state events are driven by actual native state transitions instead of transient resize messages.

| Photino.NET API | New API |
|---|---|
| `WindowCreating` | `Creating` |
| `WindowCreated` | `Created` |
| `WindowClosing` | `Closing` |
| - | `Closed` |
| `WindowLocationChanged` | `LocationChanged` |
| `WindowSizeChanged` | `SizeChanged` |
| `WindowFocusIn` | `Activated` |
| `WindowFocusOut` | `Deactivated` |
| `WindowMaximized` | `Maximized` |
| `WindowRestored` | `Restored` |
| `WindowMinimized` | `Minimized` |
| - | `FullScreenEntered` |
| - | `FullScreenExited` |
| - | `StateChanged` |

`Closing` now uses `EventHandler<CancelEventArgs>`; set `CancelEventArgs.Cancel` to cancel the close operation.

| Previous registration helper | New registration helper |
|---|---|
| `RegisterWindowCreatingHandler(...)` | `RegisterCreatingHandler(...)` |
| `RegisterWindowCreatedHandler(...)` | `RegisterCreatedHandler(...)` |
| `RegisterWindowClosingHandler(...)` | `RegisterClosingHandler(...)` |
| `RegisterFocusInHandler(...)` | `RegisterActivatedHandler(...)` |
| `RegisterFocusOutHandler(...)` | `RegisterDeactivatedHandler(...)` |
| - | `RegisterClosedHandler(...)` |
| - | `RegisterFullScreenEnteredHandler(...)` |
| - | `RegisterFullScreenExitedHandler(...)` |
| - | `RegisterStateChangedHandler(...)` |

### Window API

`PhotinoWindow` now uses explicit `Show()`-based window creation, a more consistent fluent API, unified `WindowState` tracking, clearer window state operations, platform-specific native handles, and simplified lifecycle events.

| Previous API | New API / direction |
|---|---|
| `WaitForClose()` | `PhotinoApplication.Run(window)` for application startup; `PhotinoWindow.Show()` for explicit window creation/showing. |
| `LoadRawString(...)` | `LoadString(...)` |
| Windows-only `WindowHandle` | Platform-specific handle: `HWND`, `GtkWidget*`, or `NSWindow*` |
| No explicit closed state | `IsClosed` |
| No explicit initialization state | `IsInitialized` |
| `FullScreen`, `Maximized`, and `Minimized` properties | Unified native-driven `WindowState` with `Normal`, `Minimized`, `Maximized`, and `FullScreen` |

Notable lifecycle and API changes in PhotinoX:

| Area | API |
|---|---|
| Window lifecycle | `Show`, `Activate`, `BringToFront` |
| Window state model | `WindowState`, `StateChanged` |
| Window state commands | `Maximize`, `Minimize`, `Restore`, `SetWindowState` |
| Existing state helpers | `SetFullScreen`, `SetMaximized`, `SetMinimized` |
| Chromeless window helpers | `BeginWindowDrag`, `BeginWindowResize` |
| Window/platform state | `IsInitialized`, `IsClosed`, cross-platform `WindowHandle` |

`WindowState` replaces the previous `FullScreen`, `Maximized`, and `Minimized` properties with a single cross-platform state model. It supports `Normal`, `Minimized`, `Maximized`, and `FullScreen`, and is also used for startup state configuration.

Window state tracking is native-driven: `StateChanged`, `Maximized`, `Minimized`, `Restored`, `FullScreenEntered`, and `FullScreenExited` are raised from actual state transitions, not from transient resize messages. This avoids duplicate or misleading `Restored` notifications during operations such as resizing or minimizing from fullscreen.

`Maximize()`, `Minimize()`, and `Restore()` are new command-style APIs. Existing helpers such as `SetFullScreen(...)`, `SetMaximized(...)`, and `SetMinimized(...)` remain available and update the same underlying native state.

### Custom schemes and startup content

Custom scheme registration is stricter and more predictable. Scheme names are validated, and reserved schemes such as `http`, `https`, and `file` are rejected.

Startup content selection is explicit: `Load(...)` sets URL content and clears raw string content, while `LoadString(...)` sets raw string content and clears URL content.

| Area | Behavior |
|---|---|
| `RegisterCustomSchemeHandler(...)` | Validates scheme names and rejects reserved schemes. |
| Managed custom scheme responses | Response data is backed by native-owned memory for safer managed/native interop. |
| `Load(...)` | Sets startup URL content and clears raw string content. |
| `LoadString(...)` | Sets raw string content and clears startup URL content. |

### Compatibility

These changes may require source-level updates for applications that use older Photino.NET event names, `WaitForClose()`-based startup, focus-in/focus-out event handlers, old bool-returning close handlers, the previous separate fullscreen/maximized/minimized state model now replaced by `WindowState`, or code that relied on duplicate `Restored` notifications from transient native resize messages.

### Native runtime foundation

The managed API is built on the updated `PhotinoX.Native` runtime, including safer native memory ownership, clearer platform isolation, improved interop layout, an application-oriented native message-loop model, and unified native window state tracking.

On Windows, fullscreen is handled as a native restore-aware state transition: the previous window style and placement are preserved before entering fullscreen and restored when leaving fullscreen. Startup state is synchronized without raising user callbacks before window creation completes.

On Linux Wayland, top-level window position is compositor-controlled. Move notifications and position restore are best-effort; state and size tracking remain supported.

## Core (ecosystem)

- [**PhotinoX.Native**](https://github.com/ivanvoyager/PhotinoX.Native) - native binaries for Windows/macOS/Linux.
- [**PhotinoX.Blazor**](https://github.com/ivanvoyager/PhotinoX.Blazor) - Blazor integration for native desktop apps.
- [**PhotinoX.Server**](https://github.com/ivanvoyager/PhotinoX.Server) - optional local static-file server for SPA/static assets.
- [**PhotinoX.Samples**](https://github.com/ivanvoyager/PhotinoX.Samples) - sample projects showcasing common scenarios.

---

## Install

```bash
dotnet add package PhotinoX
```
`PhotinoX.Native` provides the native WebView host binaries and must be available for the target runtime identifier.
> Package targets **net8.0; net9.0; net10.0**. CI builds use the latest **.NET 10 SDK**.

## Samples

See real, working examples here:
- [PhotinoX.Samples](https://github.com/ivanvoyager/PhotinoX.Samples)
- [PhotinoX.Blazor](https://github.com/ivanvoyager/PhotinoX.Blazor) (with Blazor support, samples inside `Samples/`)

Original Photino concept docs: https://docs.tryphotino.io/

## Requirements

- **.NET 10 SDK** (build)
- **Target frameworks:** `net8.0; net9.0; net10.0` (package supports all three)
- Runtime deps: see [**PhotinoX.Native**](https://www.nuget.org/packages/PhotinoX.Native) (`runtimes/<rid>/native/`)
- **Windows:** WebView2 Runtime  
  Required component: **Microsoft.Web.WebView2** (Edge WebView2)  
  https://learn.microsoft.com/microsoft-edge/webview2/
- **macOS**: WKWebView (system WebKit)  
  https://developer.apple.com/documentation/webkit/wkwebview/
- **Linux:** WebKitGTK 4.1 runtime packages  
  https://webkitgtk.org/

## Build from source

```bash
dotnet restore Photino.NET/PhotinoX.csproj
dotnet build   Photino.NET/PhotinoX.csproj -c Release
dotnet pack    Photino.NET/PhotinoX.csproj -c Release -o artifacts
```
> CI: see [`.github/workflows/build.yml`](https://github.com/ivanvoyager/PhotinoX/blob/master/.github/workflows/build.yml) (build + pack + upload `.nupkg`/`.snupkg`).

## Contributing

Issues and PRs are welcome. Keep PRs focused, minimal, and consistent with the rest of PhotinoX.

## License

PhotinoX is licensed under **Apache‑2.0**.