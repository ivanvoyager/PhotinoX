using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Photino.NET.NativeMethods;

namespace Photino.NET;

/// <summary>
/// Provides extension methods for <see cref="PhotinoApplication"/>.
/// </summary>
public static class PhotinoApplicationExtensions
{
    extension(PhotinoApplication application)
    {
        /// <summary>
        /// Gets the loaded PhotinoX native runtime version.
        /// </summary>
        /// <returns>
        /// The native runtime version string, or <see langword="null"/> when the native runtime
        /// does not provide version information.
        /// </returns>
        public string? GetNativeVersion()
        {
            return PtrToStringUTF8(Photino_GetNativeVersion());
        }

        /// <summary>
        /// Gets a snapshot of the current PhotinoX runtime environment.
        /// </summary>
        /// <returns>
        /// A <see cref="PhotinoRuntimeInfo"/> value containing operating system,
        /// process, native runtime, WebView engine, and platform-specific runtime details.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current operating system is not supported by PhotinoX,
        /// or when the native runtime information ABI does not match the managed ABI.
        /// </exception>
        public PhotinoRuntimeInfo GetRuntimeInfo()
        {
            var nativeInfo = Photino_GetRuntimeInfo();
            var size = Marshal.SizeOf<PhotinoNativeRuntimeInfo>();
            Debug.Assert(size == nativeInfo.Size && nativeInfo.AbiVersion == PhotinoNativeRuntimeInfo.NativeAbiVersion);

            if (nativeInfo.Size != size || nativeInfo.AbiVersion != PhotinoNativeRuntimeInfo.NativeAbiVersion)
            {
                throw new InvalidOperationException(
                    $"Unsupported PhotinoX native runtime info ABI. " +
                    $"Expected size {size}, ABI version {PhotinoNativeRuntimeInfo.NativeAbiVersion}; " +
                    $"got size {nativeInfo.Size}, ABI version {nativeInfo.AbiVersion}.");
            }

            var platform = GetPlatform();
            return new PhotinoRuntimeInfo(
                OSDescription: RuntimeInformation.OSDescription,
                OSArchitecture: RuntimeInformation.OSArchitecture.ToString(),
                ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString(),
                FrameworkDescription: RuntimeInformation.FrameworkDescription,
                NativeVersion: PtrToStringUTF8(nativeInfo.NativeVersion) ?? "Unknown",
                WebViewEngine: PtrToStringUTF8(nativeInfo.WebViewEngine) ?? "Unknown",
                WebViewRuntimeVersion: PtrToStringUTF8(nativeInfo.WebViewRuntimeVersion),
                Platform: platform,
                Windows: platform == PhotinoRuntimePlatform.Windows ? GetWindowsRuntimeInfo(nativeInfo) : default,
                Linux: platform == PhotinoRuntimePlatform.Linux ? GetLinuxRuntimeInfo(nativeInfo) : default,
                MacOS: platform == PhotinoRuntimePlatform.MacOS ? GetMacOSRuntimeInfo(nativeInfo) : default
            );

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static PhotinoWindowsRuntimeInfo GetWindowsRuntimeInfo(PhotinoNativeRuntimeInfo nativeInfo) => new(PtrToStringUTF8(nativeInfo.Windows.WebView2RuntimeVersion));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static PhotinoLinuxRuntimeInfo GetLinuxRuntimeInfo(PhotinoNativeRuntimeInfo nativeInfo)
            {
                return new PhotinoLinuxRuntimeInfo(
                    PtrToStringUTF8(nativeInfo.Linux.GlibcVersion),
                    PtrToStringUTF8(nativeInfo.Linux.GtkVersion),
                    PtrToStringUTF8(nativeInfo.Linux.WebKitGtkApiTarget),
                    PtrToStringUTF8(nativeInfo.Linux.WebKitGtkRuntimeVersion)
                    );
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static PhotinoMacOSRuntimeInfo GetMacOSRuntimeInfo(PhotinoNativeRuntimeInfo nativeInfo)
            {
                return new PhotinoMacOSRuntimeInfo(PtrToStringUTF8(nativeInfo.MacOS.WebKitVersion));
            }
        }

        private static PhotinoRuntimePlatform GetPlatform()
        {
            if (Platform.IsWindows) return PhotinoRuntimePlatform.Windows;
            if (Platform.IsLinux) return PhotinoRuntimePlatform.Linux;
            if (Platform.IsMacOS) return PhotinoRuntimePlatform.MacOS;
            throw new InvalidOperationException("Unsupported platform");
        }
    }
}