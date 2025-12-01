List<int> inputs =
    File.ReadLines("input.txt")
    .Select(s => int.Parse(s.Trim()))
    .ToList();

var values = new Dictionary<Coord, int>() { [(0, 0)] = 1 };

foreach (int input in inputs)
{
    var z = GetCoord(input);
    int d = Math.Abs(z.X) + Math.Abs(z.Y);
    Console.WriteLine($"input:{input} @{z} dist:{d}");

    var result = EnumerateValues().First(z => z.Value > input);
    Console.WriteLine($"value:{result.Value} @{result.Coord}\n");
}

IEnumerable<(Coord Coord,int Value)> EnumerateValues()
{
    int GetValue(Coord z) => values.TryGetValue(z, out int value) ? value : 0;
    foreach (var coord in EnumerateCoords())
    {
        if (!values.TryGetValue(coord, out int value))
        {
            value = coord.GetNeighbours().Sum(GetValue);
            values[coord] = value;
        }
        yield return (coord, value);
    }
}

static IEnumerable<Coord> EnumerateCoords()
{
    for (int n=1; ; ++n)
    {
        yield return GetCoord(n);
    }
}

static Coord GetCoord(int value)
{
    int group = TriangularRoot((value + 1) / 2);
    int groupStart = group * (group - 1) + 1;
    int groupOffset = value - groupStart;

    int r = group / 2; // row coord radius
    int i = groupOffset % group; // coord offset along row

    bool isX = groupOffset < group; // is row horizontal (or vertical)
    bool isAsc = group % 2 > 0; // is offset ascending (or descending)

    return (isX, isAsc) switch
    {
        (true, true) => (-r + i, -r),
        (true, false) => (r - i, r),
        (false, true) => (r + 1, -r + i),
        (false, false) => (-r, r - i),
    };
}

// https://en.wikipedia.org/wiki/Triangular_number#Triangular_roots_and_tests_for_triangular_numbers
static int TriangularRoot(int value) => (SquareRoot(8 * (value - 1) + 1) + 1) / 2;

// https://stackoverflow.com/a/76644732/28149497
static int SquareRoot(int value)
{
    int sqrt = (int)Math.Sqrt(value);
    return (sqrt * sqrt > value) ? sqrt - 1 : sqrt;
}

static class Extensions
{
    static Coord[] neighbours = { (-1, -1), (0, -1), (+1, -1), (-1, 0), (+1, 0), (-1, +1), (0, +1), (+1, +1) };
    public static IEnumerable<Coord> GetNeighbours(this Coord z) => neighbours.Select(dz => z + dz);
}
