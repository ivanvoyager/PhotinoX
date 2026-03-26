using System.Runtime.InteropServices;

namespace Photino.NET.Utils;

partial class Platform
{
    /// <summary>
    /// macOS‑specific platform info derived from the Darwin kernel version.
    /// </summary>
    public static class MacOS
    {
        /// <summary>
        /// Darwin kernel version on macOS (e.g. 22.6.0), otherwise null.
        /// </summary>
        public static Version? DarwinVersion { get; } =
            IsMacOS ? ParseDarwin(RuntimeInformation.OSDescription) : null;

        /// <summary>
        /// True if macOS version is Sonoma (Darwin 23) or newer.
        /// </summary>
        public static bool IsSonomaOrNewer => DarwinVersion?.Major >= 23;

        /// <summary>
        /// True if macOS version is older than Sonoma.
        /// </summary>
        public static bool IsPreSonoma => DarwinVersion?.Major < 23;

        private static Version? ParseDarwin(string description)
        {
            // Typical patterns:
            // "Darwin 22.6.0"
            // "Darwin Kernel Version 22.5.0 ..."
            var parts = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
                if (Version.TryParse(part, out var version))
                    return version;

            return null;
        }
    }
}
