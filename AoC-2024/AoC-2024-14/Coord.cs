record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public static Coord operator *(Coord a, int f) => f * a;
    public static Coord operator *(int f, Coord a) => (f * a.X, f * a.Y);
    public static Coord operator %(Coord a, Coord m) => (Mod(a.X, m.X), Mod(a.Y, m.Y));
    private static int Mod(int x, int m) => x < 0 ? ((x % m) + m) % m : x % m;
    public override string ToString() => $"({X},{Y})";
}
