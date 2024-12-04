internal class Grid
{
    private readonly char[,] _grid;

    public Grid(IEnumerable<string> input)
    {
        var lines = input.ToList();
        int xs = lines.Select(x => x.Length).Distinct().Single();
        int ys = lines.Count;

        _grid = new char[xs, ys];

        foreach (Coord z in EnumerateRange(xs, ys))
        {
            _grid[z.X, z.Y] = lines[z.Y][z.X];
        }
    }

    public int XSize => _grid.GetLength(0);
    public int YSize => _grid.GetLength(1);
    public char this[Coord z] => _grid[z.X, z.Y];

    public bool IsInRange(Coord z) => z.X >= 0 && z.X < XSize && z.Y >= 0 && z.Y < YSize;
    public IEnumerable<Coord> Range() => EnumerateRange(XSize, YSize);
    private static IEnumerable<Coord> EnumerateRange(int xs, int ys) =>
        from x in Enumerable.Range(0, xs)
        from y in Enumerable.Range(0, ys)
        select new Coord(x, y);
}
