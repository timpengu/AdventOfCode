namespace IntCode;

public class InputSource<T> : IInputSource<T>
{
    private readonly Func<T> _inputSource;

    public InputSource(Func<T> inputSource)
    {
        _inputSource = inputSource;
    }

    public T ReadInput() => _inputSource();
}
