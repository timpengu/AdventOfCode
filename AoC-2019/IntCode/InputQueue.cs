using System.Collections;

namespace IntCode;

public class InputQueue<T> : IInputSource<T>, IReadOnlyCollection<T>
{
    private readonly Queue<T> _queue;

    public InputQueue() : this([]) { }
    public InputQueue(IEnumerable<T> source)
    {
        _queue = new(source);
    }

    public InputQueue(int capacity)
    {
        _queue = new(capacity);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();

    public int Count => _queue.Count;
    public int Capacity => _queue.Capacity;
    public bool Contains(T value) => _queue.Contains(value);
    public T Peek() => _queue.Peek();
    public bool TryPeek(out T? value) => _queue.TryPeek(out value);

    public void Clear() => _queue.Clear();
    public void Enqueue(T value) => _queue.Enqueue(value);

    public T ReadInput() =>
        _queue.TryDequeue(out T? input) ? input
            : throw new InvalidOperationException("No input available");
}
