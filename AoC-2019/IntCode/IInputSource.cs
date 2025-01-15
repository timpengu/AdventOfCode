namespace IntCode;

public interface IInputSource<T>
{
    T ReadInput();
}
