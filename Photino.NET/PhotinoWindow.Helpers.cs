using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Photino.NET;

partial class PhotinoWindow
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfCreating([CallerMemberName] string? callerName = null)
    {
        if (_isCreating)
            ThrowWindowIsBeingCreated(callerName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfInitializedForCall([CallerMemberName] string? callerName = null)
    {
        if (_nativeInstance != IntPtr.Zero)
            ThrowWindowAlreadyInitializedForCall(callerName);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowMustBeCreatedOnStaThread()
    {
        throw new InvalidOperationException("A Photino window must be created on an STA thread on Windows.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfClosedOrNotInitialized([CallerMemberName] string? callerName = null)
    {
        ThrowIfClosed(callerName);
        ThrowIfNotInitialized(callerName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfClosed([CallerMemberName] string? callerName = null)
    {
        if (IsClosed)
            ThrowWindowAlreadyClosed(callerName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfNotInitialized([CallerMemberName] string? callerName = null)
    {
        if (_nativeInstance == IntPtr.Zero)
            ThrowWindowNotInitialized(callerName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfClosedOrInitialized([CallerMemberName] string? memberName = null)
    {
        ThrowIfClosed(memberName);

        if (_nativeInstance != IntPtr.Zero)
            ThrowWindowAlreadyInitialized(memberName);
    }

    private static void ThrowIfNotValidWindowState(PhotinoWindowState state)
    {
        if (!Enum.IsDefined(state))
            throw new ArgumentOutOfRangeException(nameof(state), state, "Invalid window state.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowIsBeingCreated(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot be called while the native window is being created.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowAlreadyInitializedForCall(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot continue because the native window has already been initialized.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowAlreadyClosed(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot be called after the Photino window has been closed.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowNotInitialized(string? callerName)
    {
        throw new InvalidOperationException($"{callerName} cannot be called until after the Photino window is initialized.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowWindowAlreadyInitialized(string? memberName)
    {
        throw new InvalidOperationException($"{memberName} can only be set before the Photino window is initialized.");
    }
}
