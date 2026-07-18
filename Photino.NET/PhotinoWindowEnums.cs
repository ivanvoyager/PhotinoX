namespace Photino.NET;

/// <summary>
/// Identifies the edge or corner of a window that a resize operation acts on.
/// </summary>
/// <seealso cref="PhotinoWindow.BeginWindowResize(PhotinoWindowEdge)" />
public enum PhotinoWindowEdge
{
    /// <summary>
    /// The top edge.
    /// </summary>
    Top,

    /// <summary>
    /// The bottom edge.
    /// </summary>
    Bottom,

    /// <summary>
    /// The left edge.
    /// </summary>
    Left,

    /// <summary>
    /// The right edge.
    /// </summary>
    Right,

    /// <summary>
    /// The top-left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// The top-right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// The bottom-left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// The bottom-right corner.
    /// </summary>
    BottomRight
}
