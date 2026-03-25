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

`PhotinoX` is a maintained fork of Photino.NET focused on stability, compatibility, and predictable cross‑platform behavior.

## What is PhotinoX?

PhotinoX builds on the original Photino design: native desktop windows hosted by modern **Web UI technologies** (Blazor, React, Vue, Angular, etc.), without bundling a full Chromium runtime.  
It relies entirely on **OS‑native WebView engines**, keeping apps small and efficient.

> **Note:** PhotinoX is an independent fork of [tryphotino/photino.NET](https://github.com/tryphotino/photino.NET) under the Apache‑2.0 license and is **not affiliated** with the original project or organization.

## Core (ecosystem)

- [**PhotinoX.Native**](https://github.com/ivanvoyager/PhotinoX.Native) - native binaries for Windows/macOS/Linux.
- [**PhotinoX.Blazor**](https://github.com/ivanvoyager/PhotinoX.Blazor) - Blazor integration for native desktop apps.
- [**PhotinoX.Server**](https://github.com/ivanvoyager/PhotinoX.Server) - optional static-file server (avoids CORS/ESM issues).
- [**PhotinoX.Samples**](https://github.com/ivanvoyager/PhotinoX.Samples) - sample projects showcasing common scenarios.

---

## Install

```bash
dotnet add package PhotinoX
```
(Ensure `PhotinoX.Native` is available at runtime for your target RID.)
> Package targets **net8.0; net9.0; net10.0**. CI builds use the latest **.NET 10 SDK**.

## Samples

See real, working examples here:
- [PhotinoX.Samples](https://github.com/ivanvoyager/PhotinoX.Samples)
- [PhotinoX.Blazor](https://github.com/ivanvoyager/PhotinoX.Blazor) (with Blazor support, samples inside /Samples) 

Docs (original Photino concepts): https://docs.tryphotino.io/

## Requirements

- **.NET 10 SDK** (build)
- **Target frameworks:** `net8.0; net9.0; net10.0` (package supports all three)
- Runtime deps: see [**PhotinoX.Native**](https://www.nuget.org/packages/PhotinoX.Native) (`runtimes/<rid>/native/`)
- **Windows:** WebView2 Runtime  
  Required component: **Microsoft.Web.WebView2** (Edge WebView2)  
  https://learn.microsoft.com/microsoft-edge/webview2/
- **macOS**: WKWebView (system WebKit)  
  https://developer.apple.com/documentation/webkit/wkwebview/
- **Linux:** WebKitGTK 4.1 development/runtime packages  
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