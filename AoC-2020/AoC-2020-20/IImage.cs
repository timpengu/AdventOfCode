public interface IImage
{
    Coord Size { get; }
    bool this[Coord z] { get; }
}
