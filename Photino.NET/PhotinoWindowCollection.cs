using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Photino.NET;

/// <summary>
/// Represents a read-only snapshot-enumerable collection of Photino windows owned by an application.
/// </summary>
/// <remarks>
/// The collection is updated internally by the application and exposes thread-safe read access
/// to the currently registered windows.
/// </remarks>
public sealed class PhotinoWindowCollection(PhotinoApplication app) : IReadOnlyList<PhotinoWindow>, INotifyCollectionChanged
{
    private static readonly NotifyCollectionChangedEventArgs s_resetCollectionChanged = new(NotifyCollectionChangedAction.Reset);

    private readonly List<PhotinoWindow> _snapshot = [];

    /// <summary>
    /// Occurs when the collection of windows changes, such as when a window is added or removed.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count { get { lock (_snapshot) { return _snapshot.Count; } } }

    public PhotinoWindow this[int index] { get { lock (_snapshot) { return _snapshot[index]; } } }

    public List<PhotinoWindow>.Enumerator GetEnumerator()
    {
        List<PhotinoWindow> copy;
        lock (_snapshot)
        {
            copy = [.. _snapshot];
        }
        return copy.GetEnumerator();
    }

    IEnumerator<PhotinoWindow> IEnumerable<PhotinoWindow>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Add(PhotinoWindow window)
    {
        lock (_snapshot)
        {
            Debug.Assert(!_snapshot.Contains(window), "The native window is already in the managed snapshot.");
            _snapshot.Add(window);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, window));
    }

    internal void OnCollectionChanged(NotifyCollectionChangedAction action, IntPtr newItems, int newItemsCount, IntPtr oldItems, int oldItemsCount)
    {
        try
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    switch (newItemsCount)
                    {
                        case 0:
                            Debug.Assert(newItems == IntPtr.Zero);
                            return;
                        case 1:
                            var newWindow = PhotinoWindow.GetWindowFromHandle(Marshal.ReadIntPtr(newItems));
                            Debug.Assert(newWindow is not null);
                            if (newWindow is null) return;
                            lock (_snapshot)
                            {
                                Debug.Assert(!_snapshot.Contains(newWindow), "The native window is already in the managed snapshot.");
                                _snapshot.Add(newWindow);
                            }
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newWindow));
                            return;
                        default:
                            var newWindows = GetWindows(newItems, newItemsCount);
                            Debug.Assert(newWindows is not null);
                            if (newWindows is null) return;
                            lock (_snapshot)
                            {
                                _snapshot.AddRange(new ReadOnlySpan<PhotinoWindow>(newWindows, 0, newItemsCount));
                            }
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newWindows));
                            return;
                    }
                case NotifyCollectionChangedAction.Remove:
                    switch (oldItemsCount)
                    {
                        case 0:
                            Debug.Assert(oldItems == IntPtr.Zero);
                            return;
                        case 1:
                            var oldWindow = PhotinoWindow.GetWindowFromHandle(Marshal.ReadIntPtr(oldItems));
                            Debug.Assert(oldWindow is not null);
                            if (oldWindow is null) return;
                            bool removed;
                            lock (_snapshot)
                            {
                                removed = _snapshot.Remove(oldWindow);
                            }
                            Debug.Assert(removed, "The removed native window is missing from the managed snapshot.");
                            if (!removed) return;
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldWindow));
                            return;
                        default:
                            var oldWindows = GetWindows(oldItems, oldItemsCount);
                            Debug.Assert(oldWindows is not null);
                            if (oldWindows is null) return;
                            lock (_snapshot)
                            {
                                foreach (var window in oldWindows)
                                {
                                    removed = _snapshot.Remove(window);
                                    Debug.Assert(removed, "The removed native window is missing from the managed snapshot.");
                                }
                            }
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldWindows));
                            return;
                    }
                case NotifyCollectionChangedAction.Reset:
                    lock (_snapshot)
                    {
                        _snapshot.Clear();
                    }
                    OnCollectionChanged(s_resetCollectionChanged);
                    return;
            }
        }
        catch (Exception ex)
        {
            app.HandleNativeCallbackException(ex);
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        Debug.Assert(app.Dispatcher.CheckAccess(), "CollectionChanged event must be raised on the PhotinoApplication dispatcher thread.");
        CollectionChanged?.Invoke(this, e);
    }

    private static unsafe PhotinoWindow[]? GetWindows(IntPtr items, int count)
    {
        if (items == IntPtr.Zero || count <= 0) return null;

        var windows = new PhotinoWindow[count];
        var states = new ReadOnlySpan<IntPtr>((void*)items, count);

        int i = 0;
        foreach (var state in states)
        {
            Debug.Assert(state != IntPtr.Zero);
            var window = PhotinoWindow.GetWindowFromHandle(state);
            windows[i++] = window;
        }

        return windows;
    }
}
