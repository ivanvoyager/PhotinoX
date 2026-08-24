using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Photino.NET;

/// <summary>
/// Represents a 2D rectangle in a native (integer-based) coordinate system.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeRect
{
    public int x, y;
    public int width, height;
}

/// <summary>
/// The <c>NativeMonitor</c> structure is used for communicating information about the monitor setup
/// to and from native system calls. This structure is defined in a sequential layout for direct,
/// unmanaged access to the underlying memory.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NativeMonitor
{
    public NativeRect monitor;
    public NativeRect work;
    public double scale;
}

/// <summary>
/// Represents information about a monitor.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct Monitor
{
    /// <summary>
    /// The full area of the monitor.
    /// </summary>
    public readonly Rectangle MonitorArea;

    /// <summary>
    /// The working area of the monitor excluding taskbars, docked windows, and docked toolbars.
    /// </summary>
    public readonly Rectangle WorkArea;

    /// <summary>
    /// The scale factor of the monitor. Standard value is 1.0.
    /// </summary>
    public readonly double Scale;

    /// <summary>
    /// Initializes a new instance of the <see cref="Monitor"/> struct.
    /// </summary>
    /// <param name="monitor">The area of monitor.</param>
    /// <param name="work">The working area of the monitor.</param>
    /// <param name="scale">The scale factor of the monitor.</param>
    public Monitor(Rectangle monitor, Rectangle work, double scale)
    {
        MonitorArea = monitor;
        WorkArea = work;
        Scale = scale;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Monitor"/> struct using native structures.
    /// </summary>
    /// <param name="monitor">The area of monitor as <see cref="NativeRect"/></param>
    /// <param name="work">The working area as <see cref="NativeRect"/></param>
    /// <param name="scale">The scale factor of the monitor. Standard value is 1.0.</param>
    internal Monitor(NativeRect monitor, NativeRect work, double scale)
        : this(
            new Rectangle(monitor.x, monitor.y, monitor.width, monitor.height),
            new Rectangle(work.x, work.y, work.width, work.height),
            scale)
    { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Monitor"/> struct using a native monitor structure.
    /// </summary>
    /// <param name="nativeMonitor">The native monitor structure.</param>
    internal Monitor(NativeMonitor nativeMonitor)
        : this(nativeMonitor.monitor, nativeMonitor.work, nativeMonitor.scale)
    { }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(MonitorArea)}={MonitorArea}, {nameof(WorkArea)}={WorkArea}, {nameof(Scale)}={Scale:0.###}";
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct NativeThickness
{
    internal readonly int left;
    internal readonly int top;
    internal readonly int right;
    internal readonly int bottom;

    internal NativeThickness(int left, int top, int right, int bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    internal NativeThickness(Thickness thickness)
    {
        left = thickness.Left;
        top = thickness.Top;
        right = thickness.Right;
        bottom = thickness.Bottom;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct NativeLayoutRegion
{
    internal readonly int width;
    internal readonly int height;
    internal readonly NativeThickness margin;
    internal readonly HorizontalAlignment horizontalAlignment;
    internal readonly VerticalAlignment verticalAlignment;

    internal NativeLayoutRegion(int width, int height, NativeThickness margin,
        HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
        this.width = width;
        this.height = height;
        this.margin = margin;
        this.horizontalAlignment = horizontalAlignment;
        this.verticalAlignment = verticalAlignment;
    }

    internal NativeLayoutRegion(LayoutRegion region)
    {
        width = region.Width;
        height = region.Height;
        margin = new NativeThickness(region.Margin);
        horizontalAlignment = region.HorizontalAlignment;
        verticalAlignment = region.VerticalAlignment;
    }
}