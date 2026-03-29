namespace Photino.NET.Utils;

/// <summary>
/// Provides modern cross‑platform OS detection helpers.
/// </summary>
public static partial class Platform
{
    /// <summary>True when running on Windows.</summary>
    public static bool IsWindows { get; } = OperatingSystem.IsWindows();

    /// <summary>True when running on macOS.</summary>
    public static bool IsMacOS { get; } = OperatingSystem.IsMacOS();

    /// <summary>True when running on Linux.</summary>
    public static bool IsLinux { get; } = OperatingSystem.IsLinux();
}
