using System.Runtime.InteropServices;
using ClosedCallback = Photino.NET.NativeDelegates.VoidCallback;
using ClosingCallback = Photino.NET.NativeDelegates.BoolCallback;
using ContentLoadedCallback = Photino.NET.NativeDelegates.StringCallback;
using ContentLoadingCallback = Photino.NET.NativeDelegates.StringCallback;
using FocusInCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusOutCallback = Photino.NET.NativeDelegates.VoidCallback;
using FullScreenChangedCallback = Photino.NET.NativeDelegates.VoidBoolCallback;
using MaximizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using MinimizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using MovedCallback = Photino.NET.NativeDelegates.IntIntCallback;   //(int x, int y)
using NavigationStartingCallback = Photino.NET.NativeDelegates.StringBoolCallback;
using NewWindowRequestedCallback = Photino.NET.NativeDelegates.StringBoolCallback;
using ResizedCallback = Photino.NET.NativeDelegates.IntIntCallback; //(int width, int height)
using RestoredCallback = Photino.NET.NativeDelegates.VoidCallback;
using StateChangedCallback = Photino.NET.NativeDelegates.StateChangedCallback;
using WebMessageReceivedCallback = Photino.NET.NativeDelegates.StringStringCallback;
using WebResourceRequestedCallback = Photino.NET.NativeDelegates.ResourceCallback;

namespace Photino.NET;

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeCallbackParameters
{
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ClosingCallback ClosingHandler;                         //#1
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FocusInCallback FocusInHandler;                         //#2
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FocusOutCallback FocusOutHandler;                       //#3
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ResizedCallback ResizedHandler;                         //#4
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MaximizedCallback MaximizedHandler;                     //#5
    [MarshalAs(UnmanagedType.FunctionPtr)] internal RestoredCallback RestoredHandler;                       //#6
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MinimizedCallback MinimizedHandler;                     //#7
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MovedCallback MovedHandler;                             //#8
    [MarshalAs(UnmanagedType.FunctionPtr)] internal WebMessageReceivedCallback WebMessageReceivedHandler;   //#9
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ContentLoadingCallback ContentLoadingHandler;           //#10
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ContentLoadedCallback ContentLoadedHandler;             //#11
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NavigationStartingCallback NavigationStartingHandler;   //#12
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NewWindowRequestedCallback NewWindowRequestedHandler;   //#13
    [MarshalAs(UnmanagedType.FunctionPtr)] internal WebResourceRequestedCallback CustomSchemeHandler;       //#14
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ClosedCallback ClosedHandler;                           //#15
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FullScreenChangedCallback FullScreenChangedHandler;     //#16
    [MarshalAs(UnmanagedType.FunctionPtr)] internal StateChangedCallback StateChangedHandler;               //#17
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeWindowParameters
{
    ///<summary>OPTIONAL: Appears on the title bar of the native window. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? Title;//#1

    ///<summary>WINDOWS AND LINUX ONLY: OPTIONAL: Path to a local file or a URL. Icon appears on the title bar of the native window (if supported). Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? IconFile;//#2

    /// <summary>OPTIONAL: If true, window is created without a title bar or borders. This allows owner-drawn title bars and borders. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Chromeless; //#3

    /// <summary>OPTIONAL: If true, window can be displayed with transparent background where supported. Chromeless windows and alpha-based page backgrounds are typically required for full-window transparency. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Transparent; //#4

    /// <summary>
    /// WINDOWS: OPTIONAL: If true and NativeParent is set, creates a native owner relationship
    /// between this window and its logical parent. Default is false.
    /// </summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseNativeWindowOwner; //#5
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeLinuxChromelessParameters
{
    /// <summary>
    /// LINUX: OPTIONAL: Height, in logical pixels, of the native chromeless drag region measured from the WebView top edge.
    /// Set to 0 to disable native Linux chromeless drag. Default is 0.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int DragRegionHeight; //#1

    /// <summary>
    /// LINUX: OPTIONAL: Left inset, in logical pixels, excluded from the native chromeless drag region. Default is 0.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int DragRegionLeftInset; //#2

    /// <summary>
    /// LINUX: OPTIONAL: Right inset, in logical pixels, excluded from the native chromeless drag region.
    /// Use this to exclude custom title bar buttons from native drag. Default is 0.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int DragRegionRightInset; //#3

    /// <summary>
    /// LINUX: OPTIONAL: Thickness, in logical pixels, of the native chromeless resize border measured from the WebView edges.
    /// Set to 0 to disable native Linux chromeless resize borders. Default is 8.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int ResizeBorderThickness; //#4
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeGeometryParameters
{
    /// <summary>OPTIONAL: Initial window position in pixels. Default is 0. Can be overridden with UseOsDefaultLocation.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Left;       //#1

    /// <summary>OPTIONAL: Initial window position in pixels. Default is 0. Can be overridden with UseOsDefaultLocation.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Top;        //#2

    /// <summary>OPTIONAL: Initial window size in pixels. Default is 0. Can be overridden with UseOsDefaultSize.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Width;      //#3

    /// <summary>OPTIONAL: Initial window size in pixels. Default is 0. Can be overridden with UseOsDefaultSize.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Height;     //#4

    /// <summary>OPTIONAL: Initial minimum window width in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MinWidth;   //#5

    /// <summary>OPTIONAL: Initial minimum window height in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MinHeight;  //#6

    /// <summary>OPTIONAL: Initial maximum window width in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MaxWidth;   //#7

    /// <summary>OPTIONAL: Initial maximum window height in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MaxHeight;  //#8

    /// <summary>OPTIONAL: Initial native window state. Default is Normal.</summary>
    [MarshalAs(UnmanagedType.I4)] internal PhotinoWindowState WindowState; //#9

    /// <summary>OPTIONAL: If true, native window appears centered on screen. Left and Top properties are ignored. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool CenterOnInitialize; //#10

    /// <summary>OPTIONAL: If true, native window can be resized by the user. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Resizable; //#11

    /// <summary>OPTIONAL: If true, native window appears in front of other windows and cannot be hidden behind them. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Topmost; //#12

    /// <summary>OPTIONAL: If true, overrides Top and Left parameters and lets the OS position the newly created window. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseOsDefaultLocation; //#13

    /// <summary>OPTIONAL: If true, overrides Height and Width parameters and lets the OS size the newly created window. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseOsDefaultSize; //#14
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeBrowserParameters
{
    ///<summary>EITHER StartString or StartUrl Must be specified: Browser control will render this HTML string when initialized. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? StartString; //#1

    ///<summary>EITHER StartString or StartUrl Must be specified: Browser control will navigate to this URL when initialized. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? StartUrl; //#2

    ///<summary>WINDOWS: OPTIONAL: Path to store user data for browser control. Defaults is user's AppDataLocal folder.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? UserDataFolder; //#3

    ///<summary>OPTIONAL: Changes the user agent on the browser control at initialization.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? UserAgent; //#4

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
    internal string? ControlInitParameters; //#5

    ///<summary>OPTIONAL: Names of custom URL Schemes. e.g. 'app', 'custom'. Array length must be 16. Default is none.</summary>
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.LPStr, SizeConst = 16)]
    internal string[] CustomSchemeNames; //#6

    /// <summary>OPTIONAL: Initial zoom level of the native browser control. e.g. 100 = 100%. Default is 100.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Zoom;       //#7

    /// <summary>OPTIONAL: If true, user can zoom the browser control. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool ZoomEnabled; //#8

    /// <summary>OPTIONAL: If true, user can access the browser control's context menu. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool ContextMenuEnabled; //#9

    /// <summary>OPTIONAL: If true, the embedded WebView status bar is enabled where supported. On Windows, this controls the WebView2 status bar shown for link hover URLs and similar browser status text. On macOS and Linux, the value is stored but currently has no native effect. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool StatusBarEnabled; //#10

    /// <summary>OPTIONAL: If true, user can access the browser control's dev tools. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool DevToolsEnabled; //#11

    /// <summary>OPTIONAL: If true, requests for access to local resources (camera, microphone, etc.) will automatically be granted. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool GrantBrowserPermissions; //#12

    /// <summary>OPTIONAL: If true, browser control allows autoplaying media when page is loaded. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool MediaAutoplayEnabled; //#13

    /// <summary>OPTIONAL: If true, browser allows access to the local file system. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool FileSystemAccessEnabled; //#14

    /// <summary>OPTIONAL: If true, web security is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool WebSecurityEnabled; //#15

    /// <summary>OPTIONAL: If true, JavaScript clipboard access is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool JavascriptClipboardAccessEnabled; //#16

    /// <summary>OPTIONAL: If true, media stream access is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool MediaStreamEnabled; //#17

    /// <summary>OPTIONAL: If true, smooth scrolling is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool SmoothScrollingEnabled; //#18

    /// <summary>OPTIONAL: If true, certificate errors are ignored where supported. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool IgnoreCertificateErrorsEnabled; //#19
}

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoWindowNativeParameters
{
    internal const int NativeSize = 424;
    internal const int NativeAbiVersion = 5;
    internal const int MaxCustomSchemeNames = 16;

    /// <summary>Set when GetParamErrors() is called, prior to initializing the native window. It is a check to make sure the struct matches what C++ is expecting.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Size; //#1

    /// <summary>Managed/native ABI version expected by this parameter layout.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion; // #2

    ///<summary>OPTIONAL: If native window is created from another native window, this is the pointer to the parent window. It is set automatically in <see cref="PhotinoWindow.Show"/>.</summary>
    internal IntPtr NativeParent; //#3

    internal PhotinoNativeCallbackParameters Callbacks; //#4
    internal PhotinoNativeWindowParameters Window; //#5
    internal PhotinoNativeLinuxChromelessParameters LinuxChromeless; //#6
    internal PhotinoNativeGeometryParameters Geometry; //#7
    internal PhotinoNativeBrowserParameters Browser; //#8

    ///<summary>Checks the parameters to ensure they are valid before window creation. Called by PhotinoWindow prior to initializing native window.</summary>
    ///<returns>List of error strings</returns>
    internal readonly void GetParamErrors(ref List<string>? errors)
    {
        var startUrl = Browser.StartUrl;
        var startString = Browser.StartString;
        var windowIconFile = Window.IconFile;

        if (string.IsNullOrWhiteSpace(startUrl) && string.IsNullOrWhiteSpace(startString))
            (errors ??= []).Add("An initial URL or HTML string must be supplied in StartUrl or StartString for the browser control to navigate to.");

        if (!Enum.IsDefined(Geometry.WindowState))
            (errors ??= []).Add($"Invalid WindowState value: {(int)Geometry.WindowState}.");

        if (!string.IsNullOrWhiteSpace(windowIconFile) && !File.Exists(windowIconFile))
            (errors ??= []).Add($"IconFile: {windowIconFile} cannot be found.");

        if (Geometry.CenterOnInitialize && Geometry.UseOsDefaultLocation)
            (errors ??= []).Add("CenterOnInitialize cannot be used with UseOsDefaultLocation.");

        if (Geometry.Width < 0)
            (errors ??= []).Add($"Width cannot be negative. Width: {Geometry.Width}.");

        if (Geometry.Height < 0)
            (errors ??= []).Add($"Height cannot be negative. Height: {Geometry.Height}.");

        if (Geometry.MinWidth < 0)
            (errors ??= []).Add($"MinWidth cannot be negative. MinWidth: {Geometry.MinWidth}.");

        if (Geometry.MinHeight < 0)
            (errors ??= []).Add($"MinHeight cannot be negative. MinHeight: {Geometry.MinHeight}.");

        if (Geometry.MaxWidth < 0)
            (errors ??= []).Add($"MaxWidth cannot be negative. MaxWidth: {Geometry.MaxWidth}.");

        if (Geometry.MaxHeight < 0)
            (errors ??= []).Add($"MaxHeight cannot be negative. MaxHeight: {Geometry.MaxHeight}.");

        if (Geometry.MinWidth > Geometry.MaxWidth)
            (errors ??= []).Add($"MinWidth cannot be greater than MaxWidth. MinWidth: {Geometry.MinWidth}, MaxWidth: {Geometry.MaxWidth}.");

        if (Geometry.MinHeight > Geometry.MaxHeight)
            (errors ??= []).Add($"MinHeight cannot be greater than MaxHeight. MinHeight: {Geometry.MinHeight}, MaxHeight: {Geometry.MaxHeight}.");

        if (Platform.IsWindows && Window.Chromeless && (Geometry.UseOsDefaultLocation || Geometry.UseOsDefaultSize))
            (errors ??= []).Add("Chromeless cannot be used with UseOsDefaultLocation or UseOsDefaultSize on Windows. Size and location must be specified.");

        if (LinuxChromeless.DragRegionHeight < 0)
            (errors ??= []).Add($"DragRegionHeight cannot be negative. DragRegionHeight: {LinuxChromeless.DragRegionHeight}.");

        if (LinuxChromeless.DragRegionLeftInset < 0)
            (errors ??= []).Add($"DragRegionLeftInset cannot be negative. DragRegionLeftInset: {LinuxChromeless.DragRegionLeftInset}.");

        if (LinuxChromeless.DragRegionRightInset < 0)
            (errors ??= []).Add($"DragRegionRightInset cannot be negative. DragRegionRightInset: {LinuxChromeless.DragRegionRightInset}.");

        if (LinuxChromeless.ResizeBorderThickness < 0)
            (errors ??= []).Add($"ResizeBorderThickness cannot be negative. ResizeBorderThickness: {LinuxChromeless.ResizeBorderThickness}.");
    }
}