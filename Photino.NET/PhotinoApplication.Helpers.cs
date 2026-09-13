using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Photino.NET;

partial class PhotinoApplication
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfRunning([CallerMemberName] string? callerName = null)
    {
        if (IsRunning)
            ThrowApplicationAlreadyStarted(callerName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfInvalidShutdownMode(PhotinoShutdownMode shutdownMode, [CallerArgumentExpression(nameof(shutdownMode))] string? paramName = null)
    {
        if (!shutdownMode.IsValid())
            ThrowInvalidShutdownMode(shutdownMode, paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfShuttingDown()
    {
        if (IsShuttingDown)
            ThrowApplicationShuttingDown();
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowApplicationAlreadyCreated()
    {
        throw new InvalidOperationException($"Cannot create more than one {typeof(PhotinoApplication).FullName} instance.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowApplicationAlreadyRunning()
    {
        throw new InvalidOperationException("The application is already running.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowNativeWindowCannotBeMovedToStaThread()
    {
        throw new InvalidOperationException("An initialized native window cannot be moved to another application thread.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowApplicationAlreadyStarted(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot be used after the application has started.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowApplicationNotRunning()
    {
        throw new InvalidOperationException("Application is not running.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowInvalidShutdownMode(PhotinoShutdownMode shutdownMode, string? paramName)
    {
        throw new InvalidEnumArgumentException(paramName, (int)shutdownMode, typeof(PhotinoShutdownMode));
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowApplicationShuttingDown()
    {
        throw new InvalidOperationException("Cannot change ShutdownMode while the application is shutting down.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PhotinoApplication GetApplicationFromHandle(IntPtr handle)
    {
        Debug.Assert(handle != IntPtr.Zero, "The native application handle is invalid.");
        var gcHandle = GCHandle.FromIntPtr(handle);
        Debug.Assert(gcHandle.IsAllocated && gcHandle.Target is PhotinoApplication, "The native application handle does not reference a valid GCHandle instance.");
        return (PhotinoApplication)gcHandle.Target!;
    }
}