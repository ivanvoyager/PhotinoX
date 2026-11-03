using static Photino.NET.NativeMethods;

namespace Photino.NET;

partial class PhotinoApplication
{
    /// <summary>
    /// Sets the WebView2 runtime path used by Windows WebView2 initialization.
    /// </summary>
    /// <remarks>
    /// Windows only. This method must be called before any WebView2-backed window is created.
    /// On other platforms, this method has no effect.
    /// </remarks>
    /// <param name="path">The WebView2 runtime path, or <see langword="null"/> to clear it.</param>
    /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the WebView2 runtime path could not be set.
    /// </exception>
    /// <seealso href="https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution">
    /// Distribute your app and the WebView2 Runtime
    /// </seealso>
    public PhotinoApplication SetWebView2RuntimePath(string? path)
    {
        if (!Platform.IsWindows)
            return this;

        if (!Photino_SetWebView2RuntimePath(path))
            throw new InvalidOperationException("Failed to set the WebView2 runtime path.");

        return this;
    }

    /// <summary>
    /// Enables or disables automatic installation of the WebView2 runtime when it is unavailable.
    /// </summary>
    /// <remarks>
    /// Windows only. Automatic installation is disabled by default.
    /// This method must be called before any WebView2-backed window is created.
    /// On other platforms, this method has no effect.
    /// </remarks>
    /// <param name="enabled"><see langword="true"/> to enable automatic WebView2 runtime installation; otherwise, <see langword="false"/>.</param>
    public static void SetWebView2RuntimeAutoInstallEnabled(bool enabled)
    {
        if (Platform.IsWindows)
            Photino_SetWebView2RuntimeInstallAllowed(enabled ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// Determines whether the WebView2 runtime is available.
    /// </summary>
    /// <remarks>
    /// Windows only. The configured fixed-version runtime path is taken into account.
    /// This method returns <see langword="false"/> on other platforms.
    /// </remarks>
    /// <returns><see langword="true"/> when the WebView2 runtime is available; otherwise, <see langword="false"/>.</returns>
    public static bool IsWebView2RuntimeAvailable()
    {
        return Platform.IsWindows && Photino_IsWebView2RuntimeAvailable();
    }
}