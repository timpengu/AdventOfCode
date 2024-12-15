internal class Map
{
    private readonly char[,] _map;

    public Map(char[,] map)
    {
        _map = map;
    }

    public int XSize => _map.GetLength(0);
    public int YSize => _map.GetLength(1);
    public char this[Coord z]
    {
        get => _map[z.X, z.Y];
        set => _map[z.X, z.Y] = value;
    }

    public bool IsInRange(Coord z) => z.X >= 0 && z.X < XSize && z.Y >= 0 && z.Y < YSize;
    public IEnumerable<Coord> Range() => EnumerateRange(XSize, YSize);
    private static IEnumerable<Coord> EnumerateRange(int xs, int ys) =>
        from x in Enumerable.Range(0, xs)
        from y in Enumerable.Range(0, ys)
        select new Coord(x, y);
}
