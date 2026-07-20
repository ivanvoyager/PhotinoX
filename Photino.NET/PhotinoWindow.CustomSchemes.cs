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
    /// </summary>
    internal Dictionary<string, NetCustomSchemeDelegate> CustomSchemes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified URI scheme has a registered managed custom scheme handler.
    /// </summary>
    /// <param name="scheme">
    /// The URI scheme name to check, without the trailing colon.
    /// </param>
    /// <returns>
    /// <c>true</c> when a managed custom scheme handler has been registered for
    /// <paramref name="scheme"/>; otherwise, <c>false</c>.
    /// </returns>
    private bool IsCustomSchemeRegistered(string scheme)
    {
        return !string.IsNullOrWhiteSpace(scheme) &&
               CustomSchemes.ContainsKey(scheme);
    }

    /// <summary>
    /// Registers user-defined custom schemes (other than 'http', 'https' and 'file') and handler methods to receive callbacks
    /// when the native browser control encounters them.
    /// </summary>
    /// <remarks>
    /// Up to 16 unique custom scheme names can be registered before native initialization.
    /// After initialization, additional schemes may be registered if the native platform accepts them.
    /// Registering an existing scheme replaces its previous handler.
    /// </remarks>
    /// <returns>
    /// Returns the current <see cref="PhotinoWindow"/> instance.
    /// </returns>
    /// <param name="scheme">The custom scheme</param>
    /// <param name="handler"><see cref="NetCustomSchemeDelegate"/></param>
    /// <exception cref="ArgumentException">
    /// Thrown when the scheme is missing, reserved, invalid, or when no handler was provided.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the window has already been closed, when more than 16 custom schemes were set before initialization,
    /// or when the native platform fails to register the scheme after initialization.
    /// </exception>
    public PhotinoWindow RegisterCustomSchemeHandler(string scheme, NetCustomSchemeDelegate handler)
    {
        ThrowIfClosed();

        if (string.IsNullOrWhiteSpace(scheme)) throw new ArgumentException("A scheme must be provided (for example 'app' or 'custom').", nameof(scheme));

        _ = handler ?? throw new ArgumentException($"A handler with a signature matching {nameof(NetCustomSchemeDelegate)} must be supplied.", nameof(handler));

        scheme = scheme.ToLowerInvariant();

        if (scheme is "http" or "https" or "file")
            throw new ArgumentException($"The scheme '{scheme}' cannot be registered as a custom scheme.", nameof(scheme));

        if (!IsValidSchemeName(scheme))
            throw new ArgumentException($"Invalid custom scheme name: '{scheme}'.", nameof(scheme));

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
            if (!CustomSchemes.ContainsKey(scheme))
            {
                var added = false;
                Invoke(() => added = Photino_AddCustomSchemeName(_nativeInstance, scheme));
                if (!added)
                    throw new InvalidOperationException($"Failed to register custom scheme: '{scheme}'.");
            }
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
    public IntPtr OnCustomScheme(string url, out int numBytes, out IntPtr outContentType)
    {
        numBytes = 0;
        outContentType = IntPtr.Zero;

        Debug.Assert(!string.IsNullOrWhiteSpace(url));

        if (string.IsNullOrWhiteSpace(url))
            return IntPtr.Zero;

        var colonPos = url.IndexOf(':');

        Debug.Assert(colonPos >= 0, $"URL: '{url}' does not contain a colon.");

        if (colonPos < 0)
            return IntPtr.Zero;

        var scheme = url.Substring(0, colonPos).ToLowerInvariant();

        Debug.Assert(scheme is not ("http" or "https" or "file"), $"The scheme '{scheme}' cannot be registered as a custom scheme.");

        if (scheme is "http" or "https" or "file")
            return IntPtr.Zero;

        Debug.Assert(CustomSchemes.ContainsKey(scheme), $"A handler for the custom scheme '{scheme}' has not been registered.");

        if (!CustomSchemes.TryGetValue(scheme, out NetCustomSchemeDelegate? handler))
            return IntPtr.Zero;

        Stream? responseStream;
        string? contentType;
        try
        {
            responseStream = handler.Invoke(this, scheme, url, out contentType);
        }
        catch (Exception ex)
        {
            OnDispatcherUnhandledException(ex);
            return IntPtr.Zero;
        }

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
            try
            {
                responseStream.CopyTo(ms);
            }
            catch (Exception ex)
            {
                OnDispatcherUnhandledException(ex);
                return IntPtr.Zero;
            }

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
            }
            catch (Exception ex)
            {
                if (buffer != IntPtr.Zero)
                {
                    Photino_FreeMemory(buffer);
                    buffer = IntPtr.Zero;
                }
                numBytes = 0;
                OnDispatcherUnhandledException(ex);
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
