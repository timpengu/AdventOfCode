public class Tile : IImage
{
    public readonly int Id;
    private readonly bool[,] _frame;

    public Tile(int id, bool[,] frame)
    {
        Id = id;
        _frame = frame;
    }

    public Image GetImage() => new Image(this);

    public Coord Size => (_frame.GetLength(0), _frame.GetLength(1));
    public bool this[Coord z] => _frame[z.X, z.Y];
}
