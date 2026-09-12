using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Photino.NET.NativeDelegates;

namespace Photino.NET;

partial class PhotinoWindow
{
    private ref struct GCHandleScope
    {
        private GCHandle _handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GCHandleScope(object value, out IntPtr handlePtr)
        {
            _handle = GCHandle.Alloc(value);
            handlePtr = GCHandle.ToIntPtr(_handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            GCHandle handle = _handle;
            _handle = default;
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }

    private sealed class GetMonitorsState
    {
        public IntPtr NativeInstance;
        public required List<Monitor> Monitors;
    }

    private static readonly GetAllMonitorsCallback s_getAllMonitorsCallback = OnGetMonitor;

    private static byte OnGetMonitor(in NativeMonitor monitor, IntPtr value)
    {
        var handle = GCHandle.FromIntPtr(value);
        var state = (GetMonitorsState)handle.Target!;

        state.Monitors.Add(new Monitor(monitor));
        return 1;
    }
}
