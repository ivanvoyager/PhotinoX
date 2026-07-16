using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Photino.NET;

using static NativeMethods;

public partial class PhotinoWindow
{
    internal const int MaxCustomSchemeNames = 16;

    /// <summary>
    /// Provides a response stream for a user-defined custom URI scheme.
    /// </summary>
    /// <param name="sender">The <see cref="PhotinoWindow"/> instance.</param>
    /// <param name="scheme">The scheme portion of the requested URL.</param>
    /// <param name="url">The full request URL.</param>
    /// <param name="contentType">
    /// The MIME content type of the response; may be <c>null</c>.
    /// </param>
    /// <returns>
    /// A readable <see cref="Stream"/> containing the response data, or <c>null</c>
    /// to indicate that the request should be handled by the default browser logic.
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Stream"/> is consumed synchronously and will be disposed
    /// by the framework after the response is read.
    /// </remarks>
    public delegate Stream? NetCustomSchemeDelegate(object? sender, string scheme, string url, out string? contentType);

    /// <summary>
    /// Stores registered custom scheme handlers keyed by scheme name.
    /// Multiple handlers for the same scheme are aggregated.
    /// </summary>
    internal Dictionary<string, NetCustomSchemeDelegate> CustomSchemes = [];

    /// <summary>
    /// Registers user-defined custom schemes (other than 'http', 'https' and 'file') and handler methods to receive callbacks
    /// when the native browser control encounters them.
    /// </summary>
    /// <remarks>
    /// Up to 16 unique custom scheme names can be registered before native initialization.
    /// After initialization, additional handlers may be added for existing schemes.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="scheme">The custom scheme</param>
    /// <param name="handler"><see cref="NetCustomSchemeDelegate"/></param>
    /// <exception cref="ArgumentException">Thrown if no scheme or handler was provided</exception>
    /// <exception cref="InvalidOperationException">Thrown if more than 16 custom schemes were set</exception>
    public PhotinoWindow RegisterCustomSchemeHandler(string scheme, NetCustomSchemeDelegate handler)
    {
        if (string.IsNullOrWhiteSpace(scheme)) throw new ArgumentException("A scheme must be provided (for example 'app' or 'custom').", nameof(scheme));

        _ = handler ?? throw new ArgumentException("A handler (method) with a signature matching NetCustomSchemeDelegate must be supplied.", nameof(handler));

        scheme = scheme.ToLowerInvariant();

        if (!IsValidSchemeName(scheme))
            throw new ArgumentException($"Invalid custom scheme name: '{scheme}'.");

        if (_nativeInstance == IntPtr.Zero)
        {
            if (!CustomSchemes.TryGetValue(scheme, out _))
            {
                if (CustomSchemes.Count >= MaxCustomSchemeNames)
                    throw new InvalidOperationException($"No more than {MaxCustomSchemeNames} custom schemes can be set prior to initialization. Additional handlers can be added after initialization.");
            }
        }
        else
        {
            if (!CustomSchemes.ContainsKey(scheme) && !Photino_AddCustomSchemeName(_nativeInstance, scheme))
                throw new InvalidOperationException($"Failed to register custom scheme: '{scheme}'.");
        }

        CustomSchemes[scheme] = handler;

        return this;
    }

    /// <summary>
    /// Invokes registered user-defined handler methods for custom URI schemes (other than 'http','https', and 'file')
    /// when the native browser control encounters them.
    /// </summary>
    /// <param name="url">URL of the Scheme</param>
    /// <param name="numBytes">Number of bytes of the response</param>
    /// <param name="outContentType">Content type of the response</param>
    /// <returns><see cref="IntPtr"/></returns>
    /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty or consists only of white-space characters.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the URL does not contain a colon.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler is registered.
    /// </exception>
    public IntPtr OnCustomScheme(string url, out int numBytes, out IntPtr outContentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        var colonPos = url.IndexOf(':');

        if (colonPos < 0)
            throw new ArgumentException($"URL: '{url}' does not contain a colon.", nameof(url));

        var scheme = url.Substring(0, colonPos).ToLowerInvariant();

        if (!CustomSchemes.TryGetValue(scheme, out NetCustomSchemeDelegate? handler))
            throw new InvalidOperationException($"A handler for the custom scheme '{scheme}' has not been registered.");

        var responseStream = handler.Invoke(this, scheme, url, out var contentType);

        numBytes = 0;
        outContentType = IntPtr.Zero;

        if (responseStream == null)
        {
            // Webview should pass through request to normal handlers (e.g., network)
            // or handle as 404 otherwise
            return IntPtr.Zero;
        }

        // Read the stream into memory and serve the bytes
        // In the future, it would be possible to pass the stream through into C++
        using (responseStream)
        using (var ms = new MemoryStream())
        {
            responseStream.CopyTo(ms);

            if (ms.Length is <= 0 or > int.MaxValue)
                return IntPtr.Zero;

            numBytes = (int)ms.Length;
            IntPtr buffer = IntPtr.Zero;
            try
            {
                // Memory allocated here should be released by the native layer after the response is processed.
                buffer = Photino_AllocateMemory(numBytes);
                if (buffer == IntPtr.Zero)
                {
                    numBytes = 0;
                    return IntPtr.Zero;
                }

                Marshal.Copy(ms.GetBuffer(), 0, buffer, numBytes);
                outContentType = CopyUtf8StringToNative(contentType);//uses Photino_AllocateString
                //_hGlobalToFree.Add(buffer);
            }
            catch (Exception ex)
            {
                if (buffer != IntPtr.Zero)
                {
                    Photino_FreeMemory(buffer);
                    buffer = IntPtr.Zero;
                }
                numBytes = 0;
                Debug.Fail(ex.Message);
            }

            return buffer;
        }
    }

    private static bool IsValidSchemeName(string value)
    {
        if (string.IsNullOrEmpty(value) || !char.IsAsciiLetter(value[0]))
            return false;

        for (var i = 1; i < value.Length; i++)
        {
            var c = value[i];

            if (!char.IsAsciiLetterOrDigit(c) &&
                c != '+' &&
                c != '-' &&
                c != '.')
            {
                return false;
            }
        }

        return true;
    }
}
