using System.Collections.Concurrent;

namespace IntCode;

public class BlockingInputQueue<T> : IInputSource<T>
{
    private readonly BlockingCollection<T> _queue;

    public BlockingInputQueue()
    {
        _queue = new();
    }

    public BlockingInputQueue(int capacity)
    {
        _queue = new(capacity);
    }

    public int Capacity => _queue.BoundedCapacity;
    public int Count => _queue.Count;
    public bool Contains(T value) => _queue.Contains(value);    
    public void Enqueue(T value) => _queue.Add(value);
    public T ReadInput() => _queue.Take();
}
