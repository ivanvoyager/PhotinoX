using System.Runtime.InteropServices;
using System.Text;

namespace Photino.NET;

/// <summary>
/// Contains diagnostic information about the current PhotinoX runtime environment.
/// </summary>
/// <param name="OSDescription">
/// A human-readable operating system description.
/// </param>
/// <param name="OSArchitecture">
/// The operating system architecture reported by the .NET runtime.
/// </param>
/// <param name="ProcessArchitecture">
/// The current process architecture reported by the .NET runtime.
/// </param>
/// <param name="FrameworkDescription">
/// A human-readable description of the .NET runtime used by the current process.
/// </param>
/// <param name="NativeVersion">
/// The version of the loaded PhotinoX native runtime.
/// </param>
/// <param name="WebViewEngine">
/// The WebView engine used by the current platform, such as WebView2, WKWebView, or WebKitGTK.
/// </param>
/// <param name="WebViewRuntimeVersion">
/// The detected WebView runtime version, or <see langword="null"/> when it is unavailable.
/// </param>
/// <param name="Platform">
/// The PhotinoX runtime platform.
/// </param>
/// <param name="Windows">
/// Windows-specific runtime information.
/// </param>
/// <param name="Linux">
/// Linux-specific runtime information.
/// </param>
/// <param name="MacOS">
/// macOS-specific runtime information.
/// </param>
public readonly record struct PhotinoRuntimeInfo(
    string OSDescription,
    string OSArchitecture,
    string ProcessArchitecture,
    string FrameworkDescription,
    string NativeVersion, //5.0.0
    string WebViewEngine, //WebView2, WKWebView, WebKitGTK
    string? WebViewRuntimeVersion,
    PhotinoRuntimePlatform Platform,
    PhotinoWindowsRuntimeInfo Windows,
    PhotinoLinuxRuntimeInfo Linux,
    PhotinoMacOSRuntimeInfo MacOS)
{
    public override string ToString()
    {
        var builder = new StringBuilder();

        Append("OS", OSDescription);
        Append("OS architecture", OSArchitecture);
        Append("Process architecture", ProcessArchitecture);
        Append(".NET", FrameworkDescription);
        Append("Native", NativeVersion);
        Append("WebView engine", WebViewEngine);
        Append("WebView runtime", WebViewRuntimeVersion);

        switch (Platform)
        {
            case PhotinoRuntimePlatform.Windows:
                AppendDistinct("WebView2 runtime", Windows.WebView2RuntimeVersion, WebViewRuntimeVersion);
                break;

            case PhotinoRuntimePlatform.Linux:
                Append("glibc", Linux.GlibcVersion);
                Append("GTK", Linux.GtkVersion);
                Append("WebKitGTK API", Linux.WebKitGtkApiTarget);
                AppendDistinct("WebKitGTK runtime", Linux.WebKitGtkRuntimeVersion, WebViewRuntimeVersion);
                break;

            case PhotinoRuntimePlatform.MacOS:
                AppendDistinct("WebKit", MacOS.WebKitVersion, WebViewRuntimeVersion);
                break;
        }

        return builder.ToString().TrimEnd();

        void Append(string name, string? value)
        {
            builder
                .Append(name)
                .Append(": ")
                .AppendLine(string.IsNullOrWhiteSpace(value) ? "-" : value);
        }

        void AppendDistinct(string name, string? value, string? existingValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (string.Equals(value, existingValue, StringComparison.Ordinal))
                return;

            Append(name, value);
        }
    }
}

/// <summary>
/// Identifies the platform used by the current PhotinoX runtime.
/// </summary>
public enum PhotinoRuntimePlatform
{
    /// <summary>
    /// Microsoft Windows.
    /// </summary>
    Windows,

    /// <summary>
    /// Linux.
    /// </summary>
    Linux,

    /// <summary>
    /// macOS.
    /// </summary>
    MacOS
}

/// <summary>
/// Contains Windows-specific PhotinoX runtime information.
/// </summary>
/// <param name="WebView2RuntimeVersion">
/// The detected Microsoft Edge WebView2 runtime version, or <see langword="null"/> when it is unavailable.
/// </param>
public readonly record struct PhotinoWindowsRuntimeInfo(string? WebView2RuntimeVersion);

/// <summary>
/// Contains Linux-specific PhotinoX runtime information.
/// </summary>
/// <param name="GlibcVersion">
/// The detected GNU C Library runtime version, or <see langword="null"/> when it is unavailable.
/// </param>
/// <param name="GtkVersion">
/// The detected GTK runtime version, or <see langword="null"/> when it is unavailable.
/// </param>
/// <param name="WebKitGtkApiTarget">
/// The WebKitGTK API target used by the loaded native runtime, such as WebKitGTK 4.1.
/// </param>
/// <param name="WebKitGtkRuntimeVersion">
/// The detected WebKitGTK runtime version, or <see langword="null"/> when it is unavailable.
/// </param>
public readonly record struct PhotinoLinuxRuntimeInfo(string? GlibcVersion, string? GtkVersion, string? WebKitGtkApiTarget, string? WebKitGtkRuntimeVersion);

/// <summary>
/// Contains macOS-specific PhotinoX runtime information.
/// </summary>
/// <param name="WebKitVersion">
/// The detected WebKit runtime version, or <see langword="null"/> when it is unavailable.
/// </param>
public readonly record struct PhotinoMacOSRuntimeInfo(string? WebKitVersion);

[StructLayout(LayoutKind.Explicit, Size = 64)]
internal struct PhotinoNativeRuntimeInfo
{
    internal const int NativeAbiVersion = 2;

    [FieldOffset(0), MarshalAs(UnmanagedType.I4)] internal int Size; //#1
    [FieldOffset(4), MarshalAs(UnmanagedType.I4)] internal int AbiVersion; //#2

    [FieldOffset(8)] internal IntPtr NativeVersion; //#3

    [FieldOffset(16)] internal IntPtr WebViewEngine; //#4

    [FieldOffset(24)] internal IntPtr WebViewRuntimeVersion; //#5

    [FieldOffset(32)] internal PhotinoNativeWindowsRuntimeInfo Windows; //#6

    [FieldOffset(32)] internal PhotinoNativeLinuxRuntimeInfo Linux; //#6

    [FieldOffset(32)] internal PhotinoNativeMacOSRuntimeInfo MacOS; //#6
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeWindowsRuntimeInfo
{
    internal IntPtr WebView2RuntimeVersion;
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeLinuxRuntimeInfo
{
    internal IntPtr GlibcVersion;
    internal IntPtr GtkVersion;
    internal IntPtr WebKitGtkApiTarget;
    internal IntPtr WebKitGtkRuntimeVersion;
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeMacOSRuntimeInfo
{
    internal IntPtr WebKitVersion;
}