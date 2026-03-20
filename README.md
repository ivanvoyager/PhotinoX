# PhotinoX

[![NuGet Version](https://img.shields.io/nuget/v/PhotinoX.svg)](https://www.nuget.org/packages/PhotinoX)
[![Build](https://github.com/ivanvoyager/PhotinoX/actions/workflows/build.yml/badge.svg)](https://github.com/ivanvoyager/PhotinoX/actions/workflows/build.yml)
[![License](https://img.shields.io/github/license/ivanvoyager/PhotinoX?label=license)](https://github.com/ivanvoyager/PhotinoX/blob/master/LICENSE)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PhotinoX.svg)](https://www.nuget.org/packages/PhotinoX)

Lightweight **.NET wrapper** for native OS WebView windows:
- **Windows**: WebView2
- **macOS**: WKWebView
- **Linux**: WebKitGTK 4.1

`PhotinoX` is a maintained fork of Photino.NET. It keeps the original spirit (small, fast, no bundled Chromium) and works with **PhotinoX.Native**.

> **What is PhotinoX?**  
> PhotinoX is a lightweight cross‑platform framework for building native desktop apps using **Web UI technologies** (Blazor, React, Vue, Angular, etc.). It uses **OS‑native WebView implementations**, ensuring minimal footprint and maximum compatibility.  
> **Note:** PhotinoX is an independent fork of [tryphotino/photino.NET](https://github.com/tryphotino/photino.NET) under the Apache‑2.0 license and is **not affiliated** with the original project or organization.
 
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
- **Windows:** WebView2 Runtime
- **macOS:** WKWebView (system)
- **Linux:** WebKitGTK 4.1 development/runtime packages

## Build from source

```bash
dotnet restore Photino.NET/PhotinoX.csproj
dotnet build   Photino.NET/PhotinoX.csproj -c Release
dotnet pack    Photino.NET/PhotinoX.csproj -c Release -o artifacts
```
> CI: see `.github/workflows/build.yml` (build + pack + upload `.nupkg`/`.snupkg`).

## Related packages

[`PhotinoX.Native`](https://www.nuget.org/packages/PhotinoX.Native) — native binaries (`runtimes/<rid>/native/`)

## Contributing

Issues and PRs are welcome. Keep changes minimal and performance-conscious.

## License

PhotinoX is licensed under **Apache‑2.0**.  