internal record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public static Coord operator *(Coord a, int f) => f * a;
    public static Coord operator *(int f, Coord a) => new Coord(f * a.X, f * a.Y);
    public override string ToString() => $"({X},{Y})";
}
