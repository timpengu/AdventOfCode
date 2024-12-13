const char Sentinel = ' ';

Func<Coord, Coord>[] rotations =
{
    z => (z.X, z.Y),   // 0°
    z => (-z.Y, z.X),  // 90°
    z => (-z.X, -z.Y), // 180°
    z => (z.Y, -z.X),  // 270°
};

Coord neighbourOffset = (0, 1);
Coord[] neighbourOffsets = rotations.Select(rotate => rotate(neighbourOffset)).ToArray();

Coord[] sideDetectKernel = { (0, 0), (1, 0), (0, 1), (1, 1) };
Coord[][] sideDetectKernels = rotations.Select(rotate => Transform(sideDetectKernel, rotate)).ToArray();

IList<string> lines = File.ReadLines("inputSample.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

List<(char Value, int Area, int Perimeter, int Sides)> results = new(
    from region in FindRegions()
    let value = GetValue(region.First())
    let area = region.Count
    let perimeter = region.Sum(CountPerimeter)
    let sides = region.Sum(CountSides)
    select (value, area, perimeter, sides)
);

foreach (var result in results)
{
    Console.WriteLine($"Region '{result.Value}' => {result.Area} * {result.Perimeter} = {result.Area * result.Perimeter}");
}

Console.WriteLine("Total cost: {0} (by perimeter)\n", results.Sum(r => r.Area * r.Perimeter));

foreach (var result in results)
{
    Console.WriteLine($"Region '{result.Value}' => {result.Area} * {result.Sides} = {result.Area * result.Sides}");
}

Console.WriteLine("Total cost: {0} (by sides)\n", results.Sum(r => r.Area * r.Sides));


IEnumerable<ISet<Coord>> FindRegions()
{
    HashSet<Coord> visited = new();
    
    foreach (int y in Enumerable.Range(0, ys))
    foreach (int x in Enumerable.Range(0, xs))
    {
        Coord z = (x, y);
        if (!visited.Contains(z))
        {
            ISet<Coord> region = FindRegion(z);
            visited.UnionWith(region);
            yield return region;
        }
    }
}

ISet<Coord> FindRegion(Coord zStart)
{
    HashSet<Coord> visited = new();
    Queue<Coord> toVisit = new([zStart]);
    
    while (toVisit.TryDequeue(out Coord z))
    {
        if (!visited.Contains(z))
        {
            visited.Add(z);

            char value = GetValue(z);
            foreach (Coord neighbour in GetNeighbours(z).Where(zn => GetValue(zn) == value))
            {
                toVisit.Enqueue(neighbour);
            }
        }
    }

    return visited;
}

int CountPerimeter(Coord z) // counts neighbours of z outside region
{
    char value = GetValue(z);
    return GetNeighbours(z).Count(zn => GetValue(zn) != value);
}

int CountSides(Coord z) => sideDetectKernels.Count(k => HasSide(z, k)); // counts sides at z in each direction
bool HasSide(Coord z, Coord[] kernel) // detects a side at z in the direction defined by kernel
{
    char value = GetValue(z);
    bool[] isRegion = kernel.Select(dz => GetValue(z + dz) == value).ToArray();
    bool hasEdgeHere = isRegion[0] && !isRegion[1];
    bool hasEdgeNext = isRegion[2] && !isRegion[3];
    return hasEdgeHere && !hasEdgeNext;
}

IEnumerable<Coord> GetNeighbours(Coord z) => neighbourOffsets.Select(dz => z + dz);
Coord[] Transform(Coord[] zs, Func<Coord, Coord> transformation) => zs.Select(transformation).ToArray();

char GetValue(Coord z) => IsInRange(z) ? lines[z.Y][z.X] : Sentinel;
bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public override string ToString() => $"({X},{Y})";
}
