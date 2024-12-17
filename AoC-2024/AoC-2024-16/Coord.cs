record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public Coord RotateLeft() => (-Y, X);
    public Coord RotateRight() => (Y, -X);
    public override string ToString() => $"({X},{Y})";
}
