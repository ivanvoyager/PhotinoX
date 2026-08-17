using System.Runtime.InteropServices;

using static Photino.NET.NativeMethods;

namespace Photino.NET;

partial class PhotinoWindow
{
    /// <summary>
    /// Gets or sets an HTML string that the browser control will render when initialized.
    /// Default is <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Either StartString or StartUrl must be specified.
    /// </remarks>
    /// <seealso cref="StartUrl" />
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    public string? StartString
    {
        get
        {
            return _startupParameters.Browser.StartString;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.StartString = value;
        }
    }

    /// <summary>
    /// Gets or sets an URL that the browser control will navigate to when initialized.
    /// Default is none.
    /// </summary>
    /// <remarks>
    /// Either StartString or StartUrl must be specified.
    /// </remarks>
    /// <seealso cref="StartString" />
    /// <exception cref="InvalidOperationException">
    /// Thrown if trying to set value after native window is initialized.
    /// </exception>
    public string? StartUrl
    {
        get
        {
            return _startupParameters.Browser.StartUrl;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.StartUrl = value;
        }
    }

    /// <summary>
    /// When true, the user can access the browser control's context menu.
    /// By default, this is set to true.
    /// </summary>
    public bool ContextMenuEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.ContextMenuEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetContextMenuEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Browser.ContextMenuEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetContextMenuEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
        }
    }

    /// <summary>
    /// When true, the user can zoom.
    /// By default, this is set to true.
    /// </summary>
    public bool ZoomEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.ZoomEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetZoomEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Browser.ZoomEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetZoomEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
        }
    }

    /// <summary>
    /// Gets or sets whether the embedded WebView status bar is enabled.
    /// </summary>
    /// <remarks>
    /// On Windows, this maps to WebView2 status bar visibility.
    /// On macOS and Linux, this option is currently stored but has no native effect.
    /// </remarks>
    public bool StatusBarEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.StatusBarEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetStatusBarEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Browser.StatusBarEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetStatusBarEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
        }
    }

    /// <summary>
    /// When true, the user can access the browser control's developer tools.
    /// By default, this is set to true.
    /// </summary>
    public bool DevToolsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.DevToolsEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetDevToolsEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Browser.DevToolsEnabled = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetDevToolsEnabled(state.NativeInstance, (byte)(state.Value ? 1 : 0));
            }, (NativeInstance: _nativeInstance, Value: value));
        }
    }

    /// <summary>
    /// Gets or sets the native browser control <see cref="PhotinoWindow.Zoom"/>.
    /// Default is 100.
    /// </summary>
    /// <example>100 = 100%, 50 = 50%</example>
    public int Zoom
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.Zoom;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetZoom(nativeInstance, out int zoom);
                return zoom;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosed();

            if (_nativeInstance == IntPtr.Zero)
            {
                _startupParameters.Browser.Zoom = value;
                return;
            }

            Dispatcher.Invoke(static state =>
            {
                Photino_SetZoom(state.NativeInstance, state.Value);
            }, (NativeInstance: _nativeInstance, Value: value));
        }
    }

    public string? UserAgent
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.UserAgent;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                var ptr = Photino_GetUserAgent(nativeInstance);
                try
                {
                    return ptr != IntPtr.Zero
                        ? Marshal.PtrToStringUTF8(ptr)
                        : null;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                        Photino_FreeString(ptr);
                }
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.UserAgent = value;
        }
    }

    /// <summary>
    /// Gets or sets platform‑specific initialization parameters for the native browser control on startup.
    /// Default is none.
    /// </summary>
    /// <remarks>
    /// The value is passed to the native browser backend during initialization.
    /// Supported format and options are platform-specific.
    /// <para><b>Windows:</b> WebView2-specific argument string (space-separated).</para>
    /// <para>https://peter.sh/experiments/chromium-command-line-switches/</para>
    /// <para>https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2environmentoptions.additionalbrowserarguments</para>
    /// <para>https://www.chromium.org/developers/how-tos/run-chromium-with-flags/</para>
    ///
    /// <para><b>Linux:</b> WebKit2GTK JSON settings.</para>
    /// <para>Example: <c>{ "set_enable_encrypted_media": true }</c></para>
    /// <para>https://webkitgtk.org/reference/webkit2gtk/2.5.1/WebKitSettings.html</para>
    /// <para>https://lazka.github.io/pgi-docs/WebKit2-4.0/classes/Settings.html</para>
    ///
    /// <para><b>macOS:</b> WebKit (WKWebView) JSON settings.</para>
    /// <para>Example: <c>{ "minimumFontSize": 8 }</c></para>
    /// <para>https://developer.apple.com/documentation/webkit/wkwebviewconfiguration</para>
    /// <para>https://developer.apple.com/documentation/webkit/wkpreferences</para>
    /// </remarks>
    public string? BrowserControlInitParameters
    {
        get
        {
            return _startupParameters.Browser.ControlInitParameters;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.ControlInitParameters = value;
        }
    }

    /// <summary>
    /// Gets or Sets whether the native browser control grants all requests for access to local resources
    /// such as the users camera and microphone. By default, this is set to true.
    /// </summary>
    /// <remarks>
    /// This only works on Windows.
    /// </remarks>
    public bool GrantBrowserPermissions
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.GrantBrowserPermissions;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetGrantBrowserPermissions(nativeInstance, out byte grant);
                return grant != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.GrantBrowserPermissions = value;
        }
    }

    public bool MediaAutoplayEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.MediaAutoplayEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetMediaAutoplayEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.MediaAutoplayEnabled = value;
        }
    }

    public bool FileSystemAccessEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.FileSystemAccessEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetFileSystemAccessEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.FileSystemAccessEnabled = value;
        }
    }

    public bool WebSecurityEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.WebSecurityEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetWebSecurityEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.WebSecurityEnabled = value;
        }
    }

    public bool JavascriptClipboardAccessEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.JavascriptClipboardAccessEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetJavascriptClipboardAccessEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.JavascriptClipboardAccessEnabled = value;
        }
    }

    public bool MediaStreamEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.MediaStreamEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetMediaStreamEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.MediaStreamEnabled = value;
        }
    }

    public bool IgnoreCertificateErrorsEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.IgnoreCertificateErrorsEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetIgnoreCertificateErrorsEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.IgnoreCertificateErrorsEnabled = value;
        }
    }

    public bool SmoothScrollingEnabled
    {
        get
        {
            if (_nativeInstance == IntPtr.Zero)
                return _startupParameters.Browser.SmoothScrollingEnabled;

            return Dispatcher.Invoke(static nativeInstance =>
            {
                Photino_GetSmoothScrollingEnabled(nativeInstance, out byte enabled);
                return enabled != 0;
            }, _nativeInstance);
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.SmoothScrollingEnabled = value;
        }
    }

    /// <summary>
    /// Gets or sets the WebView user data folder used by the native browser control.
    /// </summary>
    /// <remarks>
    /// Windows only. When set to <see langword="null"/>, the platform default WebView2 behavior is used.
    /// </remarks>
    public string? UserDataFolder
    {
        get
        {
            return _startupParameters.Browser.UserDataFolder;
        }
        set
        {
            ThrowIfClosedOrInitialized();

            _startupParameters.Browser.UserDataFolder = value;
        }
    }
}
