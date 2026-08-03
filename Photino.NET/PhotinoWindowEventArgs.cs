using System.ComponentModel;
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
/// <param name="Uri">The URI of the top-level WebView content that sent the message.</param>
public readonly record struct WebMessageReceivedEventArgs(string Message, Uri Uri);

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.NavigationStarting"/> event.
/// </summary>
/// <remarks>
/// Set <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/> to cancel the navigation.
/// </remarks>
public sealed class NavigationStartingEventArgs : CancelEventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationStartingEventArgs"/> class.
    /// </summary>
    /// <param name="uri">The URI that the WebView is about to navigate to.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="uri"/> is <see langword="null"/>.
    /// </exception>
    public NavigationStartingEventArgs(Uri uri)
    {
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
    }

    /// <summary>
    /// Gets the URI that the WebView is about to navigate to.
    /// </summary>
    public Uri Uri { get; }
}

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.NewWindowRequested"/> event.
/// </summary>
/// <param name="Uri">The URI requested for the new window.</param>
public readonly record struct NewWindowRequestedEventArgs(Uri Uri);

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.ContentLoaded"/> and
/// <see cref="PhotinoWindow.InitialContentLoaded"/> events.
/// </summary>
/// <remarks>
/// This event data describes a completed top-level WebView content load.
/// It does not indicate that a JavaScript framework, SPA route, Blazor component tree,
/// or all asynchronous page work has finished rendering.
/// </remarks>
/// <param name="Uri">
/// The URI of the top-level WebView content that finished loading.
/// </param>
public readonly record struct ContentLoadedEventArgs(Uri Uri);