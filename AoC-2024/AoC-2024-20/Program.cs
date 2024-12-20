Coord[] Directions = [(+1, 0), (0, +1), (-1, 0), (0, -1)];

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

(bool[,] isWall, Coord zStart, Coord zEnd) = Parse(lines);

List<Coord> path = FindShortestPath(zStart, zEnd).ToList();
Console.WriteLine($"Shortest path length: {path.Count - 1}");
Console.WriteLine(String.Join(" ", path));
Console.WriteLine();

int minShortcutSaving = 100;

// part 1
ShowBestShortcuts(2, minShortcutSaving);

// part 2
ShowBestShortcuts(20, minShortcutSaving);

void ShowBestShortcuts(int maxLength, int minSaving)
{
    Console.WriteLine($"Shortcuts with length <={maxLength} saving >={minSaving} steps:");

    var bestShortcuts = FindShortcuts(path, maxLength)
        .Select(s => (s.i, s.j, Saving: GetSaving(path, s.i, s.j)))
        .Where(s => s.Saving >= minSaving)
        .GroupBy(s => s.Saving)
        .Select(g => (Saving: g.Key, Count: g.Count()))
        .OrderBy(s => s.Saving)
        .ToList();

    foreach (var s in bestShortcuts)
    {
        Console.WriteLine($"{s.Count} shortcut/s save {s.Saving} steps");
    }

    int countBest = bestShortcuts.Sum(s => s.Count);
    Console.WriteLine($"\nTotal shortcuts with length <={maxLength} saving >={minSaving} steps: {countBest}\n");
}

int GetSaving(IList<Coord> path, int i, int j)
{
    int pathSkipped = j - i;
    int shortcutLength = GetDistance(path[i], path[j]);
    return pathSkipped - shortcutLength;
}

IEnumerable<(int i, int j)> FindShortcuts(IList<Coord> path, int maxShortcutLength)
{
    int minShortcutOffset = 4; // no shortcut can exist between steps closer than this
    for (int i = 0; i < path.Count - minShortcutOffset; ++i)
    {
        for (int j = i + minShortcutOffset; j < path.Count; ++j)
        {
            Coord zi = path[i];
            Coord zj = path[j];

            int onPathLength = j - i;
            int shortcutLength = GetDistance(zi, zj); // manhattan distance ignoring walls

            if (shortcutLength < onPathLength && shortcutLength <= maxShortcutLength)
            {
                yield return (i, j);
            }
        }
    }
}

IEnumerable<Coord> FindShortestPath(Coord zStart, Coord zTarget)
{
    HashSet<Coord> visited = new();
    Dictionary<Coord, Coord> next = new();
    Queue<Coord> queue = new([zTarget]); // search from target to start and then retrace path forward
    while (queue.TryDequeue(out Coord zCurr))
    {
        if (visited.Add(zCurr))
        {
            if (zCurr == zStart)
            {
                return GenerateWhile(zStart, next.ContainsKey, z => next[z]);
            }

            IEnumerable<Coord> unvisitedNeighbours = GetNeighbours(zCurr)
                .Where(zn => !visited.Contains(zn))
                .Where(zn => !next.ContainsKey(zn));

            foreach (Coord zNext in unvisitedNeighbours)
            {
                next.Add(zNext, zCurr);
                queue.Enqueue(zNext);
            }
        }
    }

    return Enumerable.Empty<Coord>(); // no path found
}

IEnumerable<T> GenerateWhile<T>(T initial, Predicate<T> hasSucessor, Func<T, T> successor)
{
    yield return initial;
    for (T item = initial; hasSucessor(item);)
    {
        item = successor(item);
        yield return item;
    }
}

static int GetDistance(Coord z1, Coord z2)
{
    Coord dz = z1 - z2;
    return Math.Abs(dz.X) + Math.Abs(dz.Y);
}

IEnumerable<Coord> GetNeighbours(Coord z) => Directions.Select(dz => z + dz).Where(z => !IsBlocked(z));
bool IsBlocked(Coord z) => !IsInRange(z) || IsWall(z);
bool IsWall(Coord z) => isWall[z.X, z.Y];
bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;

(bool[,], Coord, Coord) Parse(IList<string> lines)
{
    bool[,] isWall = new bool[xs, ys];
    Coord? start = null, end = null;
    foreach (int x in Enumerable.Range(0, xs))
    {
        foreach (int y in Enumerable.Range(0, ys))
        {
            char c = lines[y][x];
            isWall[x,y] = c == '#';

            if (c == 'S')
            {
                start = (x, y);
            }
            else if (c == 'E')
            {
                end = (x, y);
            }
        }
    }
    return (
        isWall,
        start ?? throw new InvalidDataException("No start position"),
        end ?? throw new InvalidDataException("No end position")
    );
}

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X},{Y})";
}
