using System.Collections;

namespace Photino.NET;

/// <summary>
/// Represents a read-only snapshot-enumerable collection of Photino windows owned by an application.
/// </summary>
/// <remarks>
/// The collection is updated internally by the application and exposes thread-safe read access
/// to the currently registered windows.
/// </remarks>
public sealed class PhotinoWindowCollection : IReadOnlyList<PhotinoWindow>
{
    private readonly List<PhotinoWindow> _windows = [];

    public int Count { get { lock (_windows) { return _windows.Count; } } }

    public PhotinoWindow this[int index] { get { lock (_windows) { return _windows[index]; } } }

    public List<PhotinoWindow>.Enumerator GetEnumerator()
    {
        List<PhotinoWindow> copy;
        lock (_windows)
        {
            copy = new List<PhotinoWindow>(_windows);
        }
        return copy.GetEnumerator();
    }

    IEnumerator<PhotinoWindow> IEnumerable<PhotinoWindow>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Add(PhotinoWindow window)
    {
        lock (_windows) { _windows.Add(window); }
    }

    internal bool Remove(PhotinoWindow window)
    {
        lock (_windows) { return _windows.Remove(window); }
    }

    internal void Clear()
    {
        lock (_windows) { _windows.Clear(); }
    }
}
