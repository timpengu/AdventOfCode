const char Sentinel = ' ';

Coord[] directions = { (+1, 0), (0, +1), (-1, 0), (0, -1) };

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

int CountSides(Coord z) => directions.Count(dz => HasSide(z, dz)); // counts sides at z in each direction
bool HasSide(Coord z, Coord di) // detects a side at z facing direction di
{
    char value = GetValue(z);
    bool IsInRegion(Coord z2) => GetValue(z2) == value;
    
    Coord dj = (-di.Y, di.X); // rotate 90° to form orthogonal basis di,dj
    bool hasEdgeHere = /*IsInRegion(z) && */ !IsInRegion(z + di); // detect edge at z facing di
    bool hasEdgeNext = IsInRegion(z + dj) && !IsInRegion(z + di + dj); // detect edge at (z + dj) facing di
    return hasEdgeHere && !hasEdgeNext;
}

IEnumerable<Coord> GetNeighbours(Coord z) => directions.Select(dz => z + dz);
char GetValue(Coord z) => IsInRange(z) ? lines[z.Y][z.X] : Sentinel;
bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public override string ToString() => $"({X},{Y})";
}
