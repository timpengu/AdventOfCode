namespace IntCode;

public static class InputSequence
{
    public static InputSequence<T> Empty<T>() => new InputSequence<T>([]);
    public static InputSequence<T> ToInputSequence<T>(this IEnumerable<T> source) => new InputSequence<T>(source);
}

public class InputSequence<T> : IInputSource<T>
{
    private readonly IEnumerator<T> _enumerator;

    public InputSequence(IEnumerable<T> source)
    {
        _enumerator = source.GetEnumerator();
    }

    public T ReadInput() =>
        _enumerator.MoveNext() ? _enumerator.Current
            : throw new InvalidOperationException("No input available");
}
