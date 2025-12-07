
(Coord zStart, IList<Coord> splitters) = File.ReadLines("input.txt").Parse();
int yEnd = splitters.Max(s => s.Y) + 1;

int splitCount = 0;
List<int> xBeams = [zStart.X];
for (int yBeam = zStart.Y; yBeam < yEnd; ++yBeam)
{
    ISet<int> xSplits = splitters
        .Where(s => yBeam == s.Y)
        .Where(s => xBeams.Contains(s.X))
        .Select(s => s.X)
        .ToHashSet();

    if (xSplits.Count == 0) continue;

    splitCount += xBeams.Where(xSplits.Contains).Count();

    int[] SplitX(int x) => xSplits.Contains(x) ? [x - 1, x + 1] : [x];
    xBeams = xBeams.SelectMany(SplitX).Distinct().ToList();

    Console.WriteLine($"{yBeam}: {String.Join(",", xBeams)} => {splitCount} splits");
}

Console.WriteLine($"Splits: {splitCount}\n");

var memoPaths = new Dictionary<Coord, long>();
long pathCount = CountPaths(zStart);

Console.WriteLine($"Paths: {pathCount}\n");

long CountPaths(Coord z)
{
    if (!memoPaths.TryGetValue(z, out long paths))
    {
        Coord zSplit = splitters
            .Where(s => s.X == z.X && s.Y > z.Y)
            .OrderBy(s => s.Y)
            .FirstOrDefault(z);

        if (zSplit == z)
        {
            paths = 1L;
            Console.WriteLine($"{z} => 1 path");
        }
        else
        {
            Coord zLeft = zSplit - (1, 0);
            Coord zRight = zSplit + (1, 0);
            paths = CountPaths(zLeft) + CountPaths(zRight);
            Console.WriteLine($"{z} = {zLeft}+{zRight} => {paths} paths");
        }

        memoPaths[z] = paths;
    }

    return paths;
}

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X},{Y})";
}

internal static class Extensions
{
    public static (Coord Start, IList<Coord> Splitters) Parse(this IEnumerable<string> lines)
    {
        List<Coord> start = [];
        List<Coord> splitters = [];
        
        foreach ((int y, string line) in lines.Index())
        {
            foreach (int x in Enumerable.Range(0, line.Length))
            {
                Coord z = (x, y);
                char c = line[x];
                if (c == 'S')
                {
                    start.Add(z);
                }
                if (c == '^')
                {
                    splitters.Add(z);
                }
            }
        }
        
        return (start.Single(), splitters);
    }
}
