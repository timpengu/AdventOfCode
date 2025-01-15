class InputSequence : IInputSource
{
    public static InputSequence Empty() => new InputSequence([]);

    private readonly IEnumerator<long> _enumerator;

    public InputSequence(IEnumerable<long> source)
    {
        _enumerator = source.GetEnumerator();
    }

    public long ReadInput() =>
        _enumerator.MoveNext() ? _enumerator.Current
            : throw new InvalidOperationException("No input available");
}
