
bool verbose = true;

List<Coord> blocks = new(
    File.ReadLines("inputSample.txt").Select(line =>
    {
        int[] coords = line.Split(',', 2).Select(int.Parse).ToArray();
        return new Coord(coords[0], coords[1]);
    })
);

Coord[] moves = [(0, +1), (+1, 0), (0, -1), (-1, 0)];

Coord size = new(blocks.Max(z => z.X) + 1, blocks.Max(z => z.Y) + 1);
Coord zStart = (0, 0);
Coord zEnd = size - (1, 1);

int blocksCount = blocks.Count > 1024 ? 1024 : 12; // for imput or inputSample

Console.WriteLine($"Size:{size} Start:{zStart} End:{zEnd} Blocks:{blocks.Count}");

// part 1
ISet<Coord> path = FindShortestPath(zStart, zEnd, blocks.Take(blocksCount)).ToHashSet();
ConsoleWritePath(path, blocks.Take(blocksCount));
Console.WriteLine($"Shortest path with {blocksCount} blocks: {path.Count - 1}");

// part 2
for(int i = blocksCount; i < blocks.Count; ++i)
{
    blocksCount = i + 1;
    Coord block = blocks[i];
    if (path.Contains(block)) // previous shortest path is now blocked?
    {
        Console.WriteLine($"Blocked by {block}");

        // find new shortest path
        ISet<Coord> newPath = FindShortestPath(zStart, zEnd, blocks.Take(blocksCount)).ToHashSet();
        if (newPath.Count > 0)
        {
            path = newPath;
            ConsoleWritePath(path, blocks.Take(blocksCount));
            Console.WriteLine($"Shortest path with {blocksCount} blocks: {path.Count - 1}");
        }
        else
        {
            ConsoleWritePath(path, blocks.Take(blocksCount));
            Console.WriteLine($"All paths blocked by {block}");
            break;
        }
    }
}

IReadOnlyCollection<Coord> FindShortestPath(Coord zStart, Coord zTarget, IEnumerable<Coord> blocks)
{
    HashSet<Coord> blocked = new(blocks);
    Dictionary<Coord, Node> nodes = new() { [zStart] = new Node(zStart, 0) };
    PriorityQueue<Coord, int> pq = new(nodes.Values.Select(n => (n.Z, n.Steps)));
    while (pq.TryDequeue(out Coord z, out _))
    {
        blocked.Add(z); // aka visited (blocked doubles as visited set)
        Node node = nodes[z];

        if (z == zTarget)
        {
            return TracePath(node);
        }

        IEnumerable<Coord> zNexts = GetNeighbours(z).Where(zNext => !blocked.Contains(zNext));
        foreach (Coord zNext in zNexts)
        {
            if (!nodes.TryGetValue(zNext, out Node? nodeNext))
            {
                nodeNext = new Node(zNext, int.MaxValue);
                nodes.Add(zNext, nodeNext);
            }

            int stepsToNext = node.Steps + 1;
            if (stepsToNext < nodeNext.Steps)
            {
                nodeNext.Steps = stepsToNext;
                nodeNext.Prev = node;

                pq.Remove(zNext, out _, out _);
                pq.Enqueue(zNext, stepsToNext);
            }
        }
    }

    return []; // no path found
}

IReadOnlyCollection<Coord> TracePath(Node node)
{
    Stack<Coord> path = new();
    for (Node? n = node; n != null; n = n.Prev)
    {
        path.Push(n.Z);
    }
    return path;
}

IEnumerable<Coord> GetNeighbours(Coord z) => moves.Select(dz => z + dz).Where(IsInRange);
bool IsInRange(Coord z) => z.X >= 0 && z.X < size.X && z.Y >= 0 && z.Y < size.Y;

void ConsoleWritePath(IEnumerable<Coord> path, IEnumerable<Coord> blocks)
{
    if (!verbose)
        return;

    var blockSet = blocks.ToHashSet();
    var pathSet = path.ToHashSet();
    bool isBlocked = blockSet.Intersect(pathSet).Any();
    foreach (int y in Enumerable.Range(0, size.Y))
    {
        foreach (int x in Enumerable.Range(0, size.X))
        {
            Coord z = (x, y);
            bool isPath = pathSet.Contains(z);
            bool isBlock = blockSet.Contains(z);
            Console.ForegroundColor =
                isPath && isBlock ? ConsoleColor.Cyan :
                isPath ? (isBlocked ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen) :
                isBlock ? ConsoleColor.White : ConsoleColor.DarkGray;
            Console.Write(isBlock ? '#' : isPath ? 'O' : '.');
        }
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine();
    }
}

record Node(Coord Z)
{
    public Node(Coord z, int steps) : this(z) { Steps = steps; }
    public int Steps { get; set; }
    public Node? Prev { get; set; }
}

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X},{Y})";
}
