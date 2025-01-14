class InputSequence : IInputSource
{
    public static InputSequence Empty() => new InputSequence([]);

    private readonly IEnumerator<int> _enumerator;

    public InputSequence(IEnumerable<int> source)
    {
        _enumerator = source.GetEnumerator();
    }

    public int ReadInput() =>
        _enumerator.MoveNext() ? _enumerator.Current
            : throw new InvalidOperationException("No input available");
}
