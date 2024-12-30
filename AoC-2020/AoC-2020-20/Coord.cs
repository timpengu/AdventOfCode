public record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public static Coord operator *(Coord a, int f) => f * a;
    public static Coord operator *(int f, Coord a) => (f * a.X, f * a.Y);
    public static Coord operator *(Coord a, Coord b) => (a.X * b.X, a.Y * b.Y);
    public static Coord operator /(Coord a, Coord b) => (a.X / b.X, a.Y / b.Y);
    public static Coord operator %(Coord a, Coord b) => (a.X % b.X, a.Y % b.Y);
    public static Coord Abs(Coord z) => (Math.Abs(z.X), Math.Abs(z.Y));
    public override string ToString() => $"({X},{Y})";
}
