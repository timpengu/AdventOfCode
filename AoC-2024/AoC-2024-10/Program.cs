using System.Collections.Immutable;

const int HeightStart = 0;
const int HeightEnd = 9;

Coord[] Moves = [(0, +1), (+1, 0), (0, -1), (-1, 0)];

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

int[,] heights = new int[xs, ys];
foreach(int x in Enumerable.Range(0, xs))
foreach(int y in Enumerable.Range(0, ys))
{
    heights[x,y] = lines[y][x] - '0';
}

List<(Coord ZStart, int Score, int Rating)> results = new(
    from y in Enumerable.Range(0, ys)
    from x in Enumerable.Range(0, xs)
    let zStart = new Coord(x, y)
    where GetHeight(zStart) == HeightStart
    let trails = FindTrailsFrom(zStart).ToList()
    let summitCount = trails.Select(t => t.Last()).Distinct().Count()
    select (zStart, trails.Count, summitCount)
);

foreach (var result in results)
{
    Console.WriteLine($"{result.ZStart} => score:{result.Score} rating:{result.Rating}");
}

Console.WriteLine($"\nTotal score: {results.Sum(r => r.Score)}");
Console.WriteLine($"Total rating: {results.Sum(r => r.Rating)}");

IEnumerable<IList<Coord>> FindTrailsFrom(Coord start) => ExtendTrail(ImmutableStack.Create(start));
IEnumerable<IList<Coord>> ExtendTrail(IImmutableStack<Coord> partialTrail)
{
    Coord z = partialTrail.Peek();
    int height = GetHeight(z);
    return height == HeightEnd
        ? Enumerable.Repeat(partialTrail.Reverse().ToList(), 1) // return this completed trail
        : GetNeighbours(z)
            .Where(zNext => GetHeight(zNext) == height + 1)
            .SelectMany(zNext => ExtendTrail(partialTrail.Push(zNext))); // extend trail recursively
}

IEnumerable<Coord> GetNeighbours(Coord z) => Moves.Select(dz => z + dz).Where(IsInRange);
bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;
int GetHeight(Coord z) => heights[z.X, z.Y];

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public override string ToString() => $"({X},{Y})";
}
