internal record struct Coord(int X, int Y) : IComparable<Coord>
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public override string ToString() => $"({X},{Y})";

    public int CompareTo(Coord other)
    {
        int compareY = Y.CompareTo(other.Y);
        if (compareY != 0) return compareY;

        int compareX = X.CompareTo(other.X);
        if (compareX != 0) return compareX;

        return 0;
    }
}
