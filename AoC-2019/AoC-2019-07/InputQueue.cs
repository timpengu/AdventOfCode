using System.Collections;

class InputQueue : IInputSource, IReadOnlyCollection<int>
{
    private readonly Queue<int> _queue;

    public InputQueue() : this([]) { }
    public InputQueue(IEnumerable<int> source)
    {
        _queue = new(source);
    }

    public InputQueue(int capacity)
    {
        _queue = new(capacity);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<int> GetEnumerator() => _queue.GetEnumerator();

    public int Count => _queue.Count;
    public int Capacity => _queue.Capacity;
    public bool Contains(int value) => _queue.Contains(value);
    public int Peek() => _queue.Peek();
    public bool TryPeek(out int value) => _queue.TryPeek(out value);

    public void Clear() => _queue.Clear();
    public void Enqueue(int value) => _queue.Enqueue(value);

    public int ReadInput() =>
        _queue.TryDequeue(out int input) ? input
            : throw new InvalidOperationException("No input available");
}
