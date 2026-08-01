using System.Drawing;

namespace Photino.NET;

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.StateChanged"/> event.
/// </summary>
/// <param name="OldState">The previous native window state.</param>
/// <param name="NewState">The new native window state.</param>
public readonly record struct StateChangedEventArgs(PhotinoWindowState OldState, PhotinoWindowState NewState);

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.LocationChanged"/> event.
/// </summary>
/// <param name="Location">The new native window location in pixels.</param>
public readonly record struct LocationChangedEventArgs(Point Location)
{
    /// <summary>
    /// Gets the new native window position from the left in pixels.
    /// </summary>
    public int Left => Location.X;

    /// <summary>
    /// Gets the new native window position from the top in pixels.
    /// </summary>
    public int Top => Location.Y;
}

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.SizeChanged"/> event.
/// </summary>
/// <param name="Size">The new native window size in pixels.</param>
public readonly record struct SizeChangedEventArgs(Size Size)
{
    /// <summary>
    /// Gets the new native window width in pixels.
    /// </summary>
    public int Width => Size.Width;

    /// <summary>
    /// Gets the new native window height in pixels.
    /// </summary>
    public int Height => Size.Height;
}

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.WebMessageReceived"/> event.
/// </summary>
/// <param name="Message">The message sent by the WebView content.</param>
public readonly record struct WebMessageReceivedEventArgs(string Message);