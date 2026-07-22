namespace Photino.NET;

/// <summary>
/// Provides data for the <see cref="PhotinoWindow.StateChanged"/> event.
/// </summary>
/// <param name="oldState">The previous native window state.</param>
/// <param name="newState">The new native window state.</param>
public sealed class PhotinoWindowStateChangedEventArgs(
    PhotinoWindowState oldState,
    PhotinoWindowState newState)
    : EventArgs
{
    /// <summary>
    /// Gets the previous native window state.
    /// </summary>
    public PhotinoWindowState OldState { get; } = oldState;

    /// <summary>
    /// Gets the new native window state.
    /// </summary>
    public PhotinoWindowState NewState { get; } = newState;
}