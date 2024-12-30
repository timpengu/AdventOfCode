using System.Diagnostics;

public class Image : IImage
{
    private readonly IImage _frame;

    public Image(IImage frame)
    {
        _frame = frame;
    }

    public bool this[Coord z]
    {
        get
        {
            Debug.Assert(z.X >= 0 && z.X < Size.X);
            Debug.Assert(z.Y >= 0 && z.Y < Size.Y);
            return _frame[z + (1, 1)];
        }
    }

    public Coord Size => _frame.Size - (2, 2);
}
