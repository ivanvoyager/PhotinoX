using System.Runtime.InteropServices;

namespace Photino.NET;

public sealed class PhotinoWebView2RuntimeNotFoundException : ExternalException
{
    public PhotinoWebView2RuntimeNotFoundException()
        : base("Microsoft Edge WebView2 Runtime was not found.")
    {
    }

    public PhotinoWebView2RuntimeNotFoundException(string message, int errorCode)
        : base(message, errorCode)
    {
    }
}