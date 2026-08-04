using System.Diagnostics;
using System.Runtime.InteropServices;
using ClosingCallback = Photino.NET.NativeDelegates.BoolCallback;
using ClosedCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusInCallback = Photino.NET.NativeDelegates.VoidCallback;
using FocusOutCallback = Photino.NET.NativeDelegates.VoidCallback;
using ResizedCallback = Photino.NET.NativeDelegates.IntIntCallback; //(int width, int height)
using MovedCallback = Photino.NET.NativeDelegates.IntIntCallback;   //(int x, int y)
using MaximizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using RestoredCallback = Photino.NET.NativeDelegates.VoidCallback;
using MinimizedCallback = Photino.NET.NativeDelegates.VoidCallback;
using WebMessageReceivedCallback = Photino.NET.NativeDelegates.StringStringCallback;
using WebResourceRequestedCallback = Photino.NET.NativeDelegates.ResourceCallback;
using FullScreenChangedCallback = Photino.NET.NativeDelegates.VoidBoolCallback;
using StateChangedCallback = Photino.NET.NativeDelegates.StateChangedCallback;
using NavigationStartingCallback = Photino.NET.NativeDelegates.StringBoolCallback;
using NewWindowRequestedCallback = Photino.NET.NativeDelegates.StringBoolCallback;
using ContentLoadedCallback = Photino.NET.NativeDelegates.StringCallback;

namespace Photino.NET;

[StructLayout(LayoutKind.Sequential)]
internal struct PhotinoNativeParameters
{
    internal const int NativeAbiVersion = 1;

    /// <summary>Set when GetParamErrors() is called, prior to initializing the native window. It is a check to make sure the struct matches what C++ is expecting.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Size; //#1

    /// <summary>Managed/native ABI version expected by this parameter layout.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int AbiVersion; // #2

    ///<summary>EITHER StartString or StartUrl Must be specified: Browser control will render this HTML string when initialized. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? StartString;//#3

    ///<summary>EITHER StartString or StartUrl Must be specified: Browser control will navigate to this URL when initialized. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? StartUrl;//#4

    ///<summary>OPTIONAL: Appears on the title bar of the native window. Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? Title;//#5

    ///<summary>WINDOWS AND LINUX ONLY: OPTIONAL: Path to a local file or a URL. Icon appears on the title bar of the native window (if supported). Default is none.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? WindowIconFile;//#6

    ///<summary>WINDOWS: OPTIONAL: Path to store user data for browser control. Defaults is user's AppDataLocal folder.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? UserDataFolder;//#7

    ///<summary>OPTIONAL: Changes the user agent on the browser control at initialization.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? UserAgent;//#8

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
    internal string? BrowserControlInitParameters;//#9

    ///<summary>WINDOWS: OPTIONAL: Registers the application for toast notifications. If not provided, uses Window Title.</summary>
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    internal string? NotificationRegistrationId;//#10

    ///<summary>OPTIONAL: Names of custom URL Schemes. e.g. 'app', 'custom'. Array length must be 16. Default is none.</summary>
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.LPStr, SizeConst = 16)]
    internal string[] CustomSchemeNames; //#11

    ///<summary>OPTIONAL: If native window is created from another native window, this is the pointer to the parent window. It is set automatically in <see cref="PhotinoWindow.Show"/>.</summary>
    internal IntPtr NativeParent; //#12

    [MarshalAs(UnmanagedType.FunctionPtr)] internal ClosingCallback ClosingHandler;                         //#13
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FocusInCallback FocusInHandler;                         //#14
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FocusOutCallback FocusOutHandler;                       //#15
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ResizedCallback ResizedHandler;                         //#16
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MaximizedCallback MaximizedHandler;                     //#17
    [MarshalAs(UnmanagedType.FunctionPtr)] internal RestoredCallback RestoredHandler;                       //#18
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MinimizedCallback MinimizedHandler;                     //#19
    [MarshalAs(UnmanagedType.FunctionPtr)] internal MovedCallback MovedHandler;                             //#20
    [MarshalAs(UnmanagedType.FunctionPtr)] internal WebMessageReceivedCallback WebMessageReceivedHandler;   //#21
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ContentLoadedCallback ContentLoadedHandler;             //#22
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NavigationStartingCallback NavigationStartingHandler;   //#23
    [MarshalAs(UnmanagedType.FunctionPtr)] internal NewWindowRequestedCallback NewWindowRequestedHandler;   //#24
    [MarshalAs(UnmanagedType.FunctionPtr)] internal WebResourceRequestedCallback CustomSchemeHandler;       //#25
    [MarshalAs(UnmanagedType.FunctionPtr)] internal ClosedCallback ClosedHandler;                           //#26
    [MarshalAs(UnmanagedType.FunctionPtr)] internal FullScreenChangedCallback FullScreenChangedHandler;     //#27
    [MarshalAs(UnmanagedType.FunctionPtr)] internal StateChangedCallback StateChangedHandler;               //#28


    /// <summary>OPTIONAL: Initial window position in pixels. Default is 0. Can be overridden with UseOsDefaultLocation.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Left;       //#29

    /// <summary>OPTIONAL: Initial window position in pixels. Default is 0. Can be overridden with UseOsDefaultLocation.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Top;        //#30

    /// <summary>OPTIONAL: Initial window size in pixels. Default is 0. Can be overridden with UseOsDefaultSize.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Width;      //#31

    /// <summary>OPTIONAL: Initial window size in pixels. Default is 0. Can be overridden with UseOsDefaultSize.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Height;     //#32

    /// <summary>OPTIONAL: Initial zoom level of the native browser control. e.g. 100 = 100%. Default is 100.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int Zoom;       //#33

    /// <summary>OPTIONAL: Initial minimum window width in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MinWidth;   //#34

    /// <summary>OPTIONAL: Initial minimum window height in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MinHeight;  //#35

    /// <summary>OPTIONAL: Initial maximum window width in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MaxWidth;   //#36

    /// <summary>OPTIONAL: Initial maximum window height in pixels.</summary>
    [MarshalAs(UnmanagedType.I4)] internal int MaxHeight;  //#37

    /// <summary>OPTIONAL: Initial native window state. Default is Normal.</summary>
    [MarshalAs(UnmanagedType.I4)] internal PhotinoWindowState WindowState; //#38

    /// <summary>OPTIONAL: If true, native window appears centered on screen. Left and Top properties are ignored. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool CenterOnInitialize; //#39

    /// <summary>OPTIONAL: If true, window is created without a title bar or borders. This allows owner-drawn title bars and borders. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Chromeless; //#40

    /// <summary>OPTIONAL: If true, window can be displayed with transparent background where supported. Chromeless windows and alpha-based page backgrounds are typically required for full-window transparency. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Transparent; //#41

    /// <summary>OPTIONAL: If true, user can access the browser control's context menu. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool ContextMenuEnabled; //#42

    /// <summary>OPTIONAL: If true, user can zoom the browser control. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool ZoomEnabled; //#43

    /// <summary>OPTIONAL: If true, user can access the browser control's dev tools. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool DevToolsEnabled; //#44

    /// <summary>OPTIONAL: If true, native window can be resized by the user. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Resizable; //#45

    /// <summary>OPTIONAL: If true, native window appears in front of other windows and cannot be hidden behind them. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool Topmost; //#46

    /// <summary>OPTIONAL: If true, overrides Top and Left parameters and lets the OS position the newly created window. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseOsDefaultLocation; //#47

    /// <summary>OPTIONAL: If true, overrides Height and Width parameters and lets the OS size the newly created window. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseOsDefaultSize; //#48

    /// <summary>OPTIONAL: If true, requests for access to local resources (camera, microphone, etc.) will automatically be granted. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool GrantBrowserPermissions; //#49

    /// <summary>OPTIONAL: If true, browser control allows autoplaying media when page is loaded. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool MediaAutoplayEnabled; //#50

    /// <summary>OPTIONAL: If true, browser allows access to the local file system. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool FileSystemAccessEnabled; //#51

    /// <summary>OPTIONAL: If true, web security is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool WebSecurityEnabled; //#52

    /// <summary>OPTIONAL: If true, JavaScript clipboard access is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool JavascriptClipboardAccessEnabled; //#53

    /// <summary>OPTIONAL: If true, media stream access is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool MediaStreamEnabled; //#54

    /// <summary>OPTIONAL: If true, smooth scrolling is enabled where supported. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool SmoothScrollingEnabled; //#55

    /// <summary>OPTIONAL: If true, certificate errors are ignored where supported. Default is false.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool IgnoreCertificateErrorsEnabled; //#56

    /// <summary>WINDOWS: OPTIONAL: If true, toast notifications are allowed on Windows by calling ShowNotification. Requires registering the app with Windows which is not always desirable as it creates shortcuts, etc. Default is true.</summary>
    [MarshalAs(UnmanagedType.I1)] internal bool NotificationsEnabled; //#57

    /// <summary>
    /// WINDOWS: OPTIONAL: If true and ParentInstance is set, creates a native owner relationship
    /// between this window and its logical parent. Default is false.
    /// </summary>
    [MarshalAs(UnmanagedType.I1)] internal bool UseNativeWindowOwner; //#58

    /// <summary>
    /// LINUX: OPTIONAL: Height, in logical pixels, of the native chromeless drag region measured from the WebView top edge.
    /// Set to 0 to disable native Linux chromeless drag. Default is 0.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int ChromelessDragRegionHeight; //#59

    /// <summary>
    /// LINUX: OPTIONAL: Left inset, in logical pixels, excluded from the native chromeless drag region. Default is 0.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int ChromelessDragRegionLeftInset; //#60

    /// <summary>
    /// LINUX: OPTIONAL: Right inset, in logical pixels, excluded from the native chromeless drag region.
    /// Use this to exclude custom title bar buttons from native drag. Default is 0.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int ChromelessDragRegionRightInset; //#61

    /// <summary>
    /// LINUX: OPTIONAL: Thickness, in logical pixels, of the native chromeless resize border measured from the WebView edges.
    /// Set to 0 to disable native Linux chromeless resize borders. Default is 8.
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] internal int ChromelessResizeBorderThickness; //#62


    ///<summary>Checks the parameters to ensure they are valid before window creation. Called by PhotinoWindow prior to initializing native window.</summary>
    ///<returns>List of error strings</returns>
    internal readonly void GetParamErrors(ref List<string>? errors)
    {
        var startUrl = StartUrl;
        var startString = StartString;
        var windowIconFile = WindowIconFile;

        if (string.IsNullOrWhiteSpace(startUrl) && string.IsNullOrWhiteSpace(startString))
            (errors ??= []).Add("An initial URL or HTML string must be supplied in StartUrl or StartString for the browser control to navigate to.");

        if (!Enum.IsDefined(WindowState))
            (errors ??= []).Add($"Invalid WindowState value: {(int)WindowState}.");

        if (!string.IsNullOrWhiteSpace(windowIconFile) && !File.Exists(windowIconFile))
            (errors ??= []).Add($"WindowIconFile: {windowIconFile} cannot be found.");

        if (CenterOnInitialize && UseOsDefaultLocation)
            (errors ??= []).Add("CenterOnInitialize cannot be used with UseOsDefaultLocation.");

        if (Width < 0)
            (errors ??= []).Add($"Width cannot be negative. Width: {Width}.");

        if (Height < 0)
            (errors ??= []).Add($"Height cannot be negative. Height: {Height}.");

        if (MinWidth < 0)
            (errors ??= []).Add($"MinWidth cannot be negative. MinWidth: {MinWidth}.");

        if (MinHeight < 0)
            (errors ??= []).Add($"MinHeight cannot be negative. MinHeight: {MinHeight}.");

        if (MaxWidth < 0)
            (errors ??= []).Add($"MaxWidth cannot be negative. MaxWidth: {MaxWidth}.");

        if (MaxHeight < 0)
            (errors ??= []).Add($"MaxHeight cannot be negative. MaxHeight: {MaxHeight}.");

        if (MinWidth > MaxWidth)
            (errors ??= []).Add($"MinWidth cannot be greater than MaxWidth. MinWidth: {MinWidth}, MaxWidth: {MaxWidth}.");

        if (MinHeight > MaxHeight)
            (errors ??= []).Add($"MinHeight cannot be greater than MaxHeight. MinHeight: {MinHeight}, MaxHeight: {MaxHeight}.");

        if (Platform.IsWindows && Chromeless && (UseOsDefaultLocation || UseOsDefaultSize))
            (errors ??= []).Add("Chromeless cannot be used with UseOsDefaultLocation or UseOsDefaultSize on Windows. Size and location must be specified.");

        if (ChromelessDragRegionHeight < 0)
            (errors ??= []).Add($"ChromelessDragRegionHeight cannot be negative. ChromelessDragRegionHeight: {ChromelessDragRegionHeight}.");

        if (ChromelessDragRegionLeftInset < 0)
            (errors ??= []).Add($"ChromelessDragRegionLeftInset cannot be negative. ChromelessDragRegionLeftInset: {ChromelessDragRegionLeftInset}.");

        if (ChromelessDragRegionRightInset < 0)
            (errors ??= []).Add($"ChromelessDragRegionRightInset cannot be negative. ChromelessDragRegionRightInset: {ChromelessDragRegionRightInset}.");

        if (ChromelessResizeBorderThickness < 0)
            (errors ??= []).Add($"ChromelessResizeBorderThickness cannot be negative. ChromelessResizeBorderThickness: {ChromelessResizeBorderThickness}.");
    }
}
