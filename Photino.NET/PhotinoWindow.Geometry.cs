namespace Photino.NET;

/// <summary>
/// Specifies where an element is positioned horizontally within its available layout space.
/// </summary>
public enum HorizontalAlignment
{
    /// <summary>
    /// The element is aligned to the left of its available layout space.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The element is aligned to the center of its available layout space.
    /// </summary>
    Center = 1,

    /// <summary>
    /// The element is aligned to the right of its available layout space.
    /// </summary>
    Right = 2,

    /// <summary>
    /// The element is stretched to fill its available horizontal layout space.
    /// </summary>
    Stretch = 3
}

/// <summary>
/// Specifies where an element is positioned vertically within its available layout space.
/// </summary>
public enum VerticalAlignment
{
    /// <summary>
    /// The element is aligned to the top of its available layout space.
    /// </summary>
    Top = 0,

    /// <summary>
    /// The element is aligned to the center of its available layout space.
    /// </summary>
    Center = 1,

    /// <summary>
    /// The element is aligned to the bottom of its available layout space.
    /// </summary>
    Bottom = 2,

    /// <summary>
    /// The element is stretched to fill its available vertical layout space.
    /// </summary>
    Stretch = 3
}

/// <summary>
/// Describes the thickness of a frame around a rectangular area.
/// </summary>
public readonly record struct Thickness
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> structure with the specified
    /// uniform thickness on every side.
    /// </summary>
    /// <param name="uniformLength">The uniform thickness applied to every side.</param>
    public Thickness(int uniformLength)
        : this(uniformLength, uniformLength, uniformLength, uniformLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> structure with the specified
    /// horizontal and vertical thickness values.
    /// </summary>
    /// <param name="horizontal">The thickness applied to the left and right sides.</param>
    /// <param name="vertical">The thickness applied to the top and bottom sides.</param>
    public Thickness(int horizontal, int vertical)
        : this(horizontal, vertical, horizontal, vertical)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> structure with the specified
    /// thickness for each side.
    /// </summary>
    /// <param name="left">The thickness of the left side.</param>
    /// <param name="top">The thickness of the top side.</param>
    /// <param name="right">The thickness of the right side.</param>
    /// <param name="bottom">The thickness of the bottom side.</param>
    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>
    /// Gets the thickness of the left side.
    /// </summary>
    public int Left { get; }

    /// <summary>
    /// Gets the thickness of the top side.
    /// </summary>
    public int Top { get; }

    /// <summary>
    /// Gets the thickness of the right side.
    /// </summary>
    public int Right { get; }

    /// <summary>
    /// Gets the thickness of the bottom side.
    /// </summary>
    public int Bottom { get; }

    /// <summary>
    /// Gets a value indicating whether all sides have the same thickness.
    /// </summary>
    public bool IsUniform => Left == Top &&
                             Left == Right &&
                             Left == Bottom;
}

/// <summary>
/// Describes a rectangular region positioned within an available layout area.
/// </summary>
/// <remarks>
/// <para>
/// The region margins define its available layout area. The horizontal alignment
/// positions the region between the left and right margins, and the vertical
/// alignment positions the region between the top and bottom margins.
/// </para>
/// <para>
/// <see cref="Photino.NET.HorizontalAlignment.Center"/> and
/// <see cref="Photino.NET.VerticalAlignment.Center"/> center the region within
/// the corresponding available layout area.
/// </para>
/// <para>
/// When an alignment is set to <see cref="Photino.NET.HorizontalAlignment.Stretch"/>
/// or <see cref="Photino.NET.VerticalAlignment.Stretch"/>, the region fills the
/// corresponding available layout area and its explicit width or height is ignored.
/// </para>
/// </remarks>
public readonly record struct LayoutRegion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LayoutRegion"/> structure.
    /// </summary>
    /// <param name="width">
    /// The width of the region.
    /// This value is ignored when <paramref name="horizontalAlignment"/> is
    /// <see cref="HorizontalAlignment.Stretch"/>.
    /// </param>
    /// <param name="height">
    /// The height of the region.
    /// This value is ignored when <paramref name="verticalAlignment"/> is
    /// <see cref="VerticalAlignment.Stretch"/>.
    /// </param>
    /// <param name="margin">
    /// The margin used to position and constrain the region within its available layout area.
    /// </param>
    /// <param name="horizontalAlignment">
    /// The horizontal alignment of the region within its available layout area.
    /// </param>
    /// <param name="verticalAlignment">
    /// The vertical alignment of the region within its available layout area.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/> or <paramref name="height"/> is negative.
    /// </exception>
    public LayoutRegion(
        int width,
        int height,
        Thickness margin = default,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment verticalAlignment = VerticalAlignment.Top)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(width);
        ArgumentOutOfRangeException.ThrowIfNegative(height);

        Width = width;
        Height = height;
        Margin = margin;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
    }

    /// <summary>
    /// Gets the width of the region.
    /// </summary>
    /// <remarks>
    /// This value is ignored when <see cref="HorizontalAlignment"/> is
    /// <see cref="Photino.NET.HorizontalAlignment.Stretch"/>.
    /// </remarks>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the region.
    /// </summary>
    /// <remarks>
    /// This value is ignored when <see cref="VerticalAlignment"/> is
    /// <see cref="Photino.NET.VerticalAlignment.Stretch"/>.
    /// </remarks>
    public int Height { get; }

    /// <summary>
    /// Gets the margin used to position and constrain the region within its available layout area.
    /// </summary>
    public Thickness Margin { get; }

    /// <summary>
    /// Gets the horizontal alignment of the region within its available layout area.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; }

    /// <summary>
    /// Gets the vertical alignment of the region within its available layout area.
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; }
}