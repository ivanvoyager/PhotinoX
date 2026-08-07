using static Photino.NET.NativeMethods;

namespace Photino.NET;

public static class PhotinoApplicationExtensions
{
    extension(PhotinoApplication application)
    {
        /// <summary>
        /// Sets the WebView2 runtime path used by Windows WebView2 initialization.
        /// </summary>
        /// <remarks>
        /// Windows only. This method must be called before any WebView2-backed window is created.
        /// </remarks>
        /// <param name="path">The WebView2 runtime path, or <see langword="null"/> to clear it.</param>
        /// <returns>The current <see cref="PhotinoApplication"/> instance.</returns>
        /// https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution
        public PhotinoApplication SetWebView2RuntimePath(string? path)
        {
            ArgumentNullException.ThrowIfNull(application);

            if (Platform.IsWindows)
                Photino_setWebView2RuntimePath_win32(path);

            return application;
        }
    }
}
