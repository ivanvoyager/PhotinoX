using System.Runtime.InteropServices;
using Photino.NET.Utils;
using ClosingCallback = Photino.NET.NativeDelegates.BoolCallback;
using ClosedCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusInCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusOutCallback = Photino.NET.NativeDelegates.VoidCallback;
using ResizedCallback = Photino.NET.NativeDelegates.IntIntCallback; //(int width, int height)
using MovedCallback = Photino.NET.NativeDelegates.IntIntCallback;   //(int x, int y)
using MaximizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using RestoredCallback = Photino.NET.NativeDelegates.VoidCallback;
using MinimizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using WebMessageReceivedCallback = Photino.NET.NativeDelegates.StringCallback;
using WebResourceRequestedCallback = Photino.NET.NativeDelegates.ResourceCallback;

namespace Photino.NET;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct PhotinoNativeParameters
{
    ///<summary>EITHER StartString or StartUrl Must be specified: Browser control will render this HTML string when initialized. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? StartString;//#1

    ///<summary>EITHER StartString or StartUrl Must be specified: Browser control will navigate to this URL when initialized. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? StartUrl;//#2

    ///<summary>OPTIONAL: Appears on the title bar of the native window. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? Title;//#3

    ///<summary>WINDOWS AND LINUX ONLY: OPTIONAL: Path to a local file or a URL. Icon appears on the title bar of the native window (if supported). Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? WindowIconFile;//#4

    ///<summary>WINDOWS: OPTIONAL: Path to store temp files for browser control. Defaults is user's AppDataLocal folder.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? TemporaryFilesPath;//#5

    ///<summary>OPTIONAL: Changes the user agent on the browser control at initialization.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? UserAgent;//#6

    ///<summary>OPTIONAL: 
    ///WINDOWS: WebView2 specific string.
    ///https://peter.sh/experiments/chromium-command-line-switches/
    ///https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2environmentoptions.additionalbrowserarguments
    ///https://www.chromium.org/developers/how-tos/run-chromium-with-flags/
    ///LINUX: Webkit2Gtk specific string.
    ///https://webkitgtk.org/reference/webkit2gtk/2.5.1/WebKitSettings.html
    ///https://lazka.github.io/pgi-docs/WebKit2-4.0/classes/Settings.html
    ///MAC: Webkit specific string.
    ///https://developer.apple.com/documentation/webkit/wkwebviewconfiguration?language=objc
    ///https://developer.apple.com/documentation/webkit/wkpreferences?language=objc
    ///</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? BrowserControlInitParameters;//#7

    ///<summary>WINDOWS: OPTIONAL: Registers the application for toast notifications. If not provided, uses Window Title.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? NotificationRegistrationId;//#8

    ///<summary>OPTIONAL: Names of custom URL Schemes. e.g. 'app', 'custom'. Array length must be 16. Default is none.</summary>
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.LPStr, SizeConst = 16)]
    internal string[] CustomSchemeNames;//#9

    ///<summary>OPTIONAL: If native window is created from another native window, this is the pointer to the parent window. It is set automatically in <see cref="PhotinoWindow.WaitForClose"/>.</summary>
    internal IntPtr NativeParent;//#10

    [MarshalAs(UnmanagedType.FunctionPtr)] internal ClosingCallback ClosingHandler;//#11
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FocusInCallback FocusInHandler;//#12
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FocusOutCallback FocusOutHandler;//#13
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ResizedCallback ResizedHandler;//#14
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MaximizedCallback MaximizedHandler;//#15
    [MarshalAs(UnmanagedType.FunctionPtr)] internal RestoredCallback RestoredHandler;//#16
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MinimizedCallback MinimizedHandler;//#17
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MovedCallback MovedHandler;//#18
    [MarshalAs(UnmanagedType.FunctionPtr)] internal WebMessageReceivedCallback WebMessageReceivedHandler;//#19
    [MarshalAs(UnmanagedType.FunctionPtr)] internal WebResourceRequestedCallback CustomSchemeHandler;//#20
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ClosedCallback ClosedHandler;//#21


    ///<summary>OPTIONAL: Initial window position in pixels. Default is 0. Can be overridden with UseOsDefaultLocation.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Left;//#22

    ///<summary>OPTIONAL: Initial window position in pixels. Default is 0. Can be overridden with UseOsDefaultLocation.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Top;//#23

    ///<summary>OPTIONAL: Initial window size in pixels. Default is 0. Can be overridden with UseOsDefaultSize.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Width;//#24

    ///<summary>OPTIONAL: Initial window size in pixels. Default is. Can be overridden with UseOsDefaultSize.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Height;//#25

    ///<summary>OPTIONAL: Initial zoom level of the native browser control. e.g.100 = 100%  Default is 100.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Zoom;//#26

    ///<summary>OPTIONAL: Initial minimum window width in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MinWidth;//#27

    ///<summary>OPTIONAL: Initial minimum window height in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MinHeight;//#28

    ///<summary>OPTIONAL: Initial maximum window width in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MaxWidth;//#29

    ///<summary>OPTIONAL: Initial maximum window height in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MaxHeight;//#30



    ///<summary>OPTIONAL: If true, native window appears in centered on screen. Left and Top properties are ignored. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool CenterOnInitialize;//#31

    ///<summary>OPTIONAL: If true, window is created without a title bar or borders. This allows owner-drawn title bars and borders. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Chromeless;//#32

    ///<summary>OPTIONAL: If true, window can be displayed with transparent background. Chromeless must be set to true. Html document's body background must have alpha-based value. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Transparent;//#33

    ///<summary>OPTIONAL: If true, user can access the browser control's context menu. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool ContextMenuEnabled;//#34

    ///<summary>OPTIONAL: If true, user can zoom the browser control. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool ZoomEnabled;//#35

    ///<summary>OPTIONAL: If true, user can access the browser control's dev tools. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool DevToolsEnabled;//#36

    ///<summary>OPTIONAL: If true, native browser control covers the entire screen. Useful for kiosks for example. Incompatible with Maximized and Minimized. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool FullScreen;//#37

    ///<summary>OPTIONAL: If true, native window is maximized to fill the screen. Incompatible with Minimized and FullScreen. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Maximized;//#38

    ///<summary>OPTIONAL: If true, native window is minimized (hidden). Incompatible with Maximized and FullScreen. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Minimized;//#39

    ///<summary>OPTIONAL: If true, native window cannot be resized by the user. Can still be resized by the program. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Resizable;//#40

    ///<summary>OPTIONAL: If true, native window appears in front of other windows and cannot be hidden behind them. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Topmost;//#41

    ///<summary>OPTIONAL: If true, overrides Top and Left parameters and lets the OS size the newly created window. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseOsDefaultLocation;//#42

    ///<summary>OPTIONAL: If true, overrides Height and Width parameters and lets the OS position the newly created window. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseOsDefaultSize;//#43

    ///<summary>OPTIONAL: If true, requests for access to local resources (camera, microphone, etc.) will automatically be granted. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool GrantBrowserPermissions;//#44

    ///<summary>OPTIONAL: If true, browser control allows autoplaying media when page is loaded. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool MediaAutoplayEnabled;//#45

    ///<summary>OPTIONAL: If true, browser allows access to the local file system. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool FileSystemAccessEnabled;//#46

    ///<summary>OPTIONAL: If true, ??? Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool WebSecurityEnabled;//#47

    ///<summary>OPTIONAL: If true, ??? Default is v.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool JavascriptClipboardAccessEnabled;//#48

    ///<summary>OPTIONAL: If true, ??? Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool MediaStreamEnabled;//#49

    ///<summary>OPTIONAL: If true, ??? Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool SmoothScrollingEnabled;//#50

    ///<summary>OPTIONAL: If true, ??? Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool IgnoreCertificateErrorsEnabled;//#51

    ///<summary>WINDOWS: OPTIONAL: If true, toast notifications are allowed on Windows by calling ShowNotification. Requires registering the app with Windows which is not always desirable as it creates shortcuts, etc. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool NotificationsEnabled;//#52


    ///<summary>Set when GetParamErrors() is called, prior to initializing the native window. It is a check to make sure the struct matches what C++ is expecting.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Size;//#53


    ///<summary>Checks the parameters to ensure they are valid before window creation. Called by PhotinoWindow prior to initializing native window.</summary>
    ///<returns>List of error strings</returns>
    internal void GetParamErrors(ref List<string>? errors)
    {
        var startUrl = StartUrl;
        var startString = StartString;
        var windowIconFile = WindowIconFile;

        if (string.IsNullOrWhiteSpace(startUrl) && string.IsNullOrWhiteSpace(startString))
            (errors ??= []).Add("An initial URL or HTML string must be supplied in StartUrl or StartString for the browser control to naviage to.");

        if (Maximized && Minimized)
            (errors ??= []).Add("Window cannot be both maximized and minimized on startup.");

        if (FullScreen && (Maximized || Minimized))
            (errors ??= []).Add("FullScreen cannot be combined with Maximized or Minimized");

        if (!string.IsNullOrWhiteSpace(windowIconFile) && !File.Exists(windowIconFile))
            (errors ??= []).Add($"WindowIconFile: {windowIconFile} cannot be found");

        if (Platform.IsWindows && Chromeless && (UseOsDefaultLocation || UseOsDefaultSize))
            (errors ??= []).Add($"Chromeless cannot be used with UseOsDefaultLocation or UseOsDefaultSize on Windows. Size and location must be specified.");

        Size = Marshal.SizeOf<PhotinoNativeParameters>();
    }
}
