using System.Collections.Immutable;

bool verbose = true;

List<Coord> blocks = new(
    File.ReadLines("input.txt").Select(line =>
    {
        int[] coords = line.Split(',', 2).Select(int.Parse).ToArray();
        return new Coord(coords[0], coords[1]);
    })
);

Coord[] moves = [(0, +1), (+1, 0), (0, -1), (-1, 0)];

Coord size = new(blocks.Max(z => z.X) + 1, blocks.Max(z => z.Y) + 1);
Coord zStart = (0, 0);
Coord zEnd = size - (1, 1);

int blocksCount = blocks.Count > 1024 ? 1024 : 12; // for input or inputSample

Console.WriteLine($"Size:{size} Start:{zStart} End:{zEnd} Blocks:{blocks.Count}");

// part 1
ISet<Coord> path = FindShortestPath(zStart, zEnd, blocks.Take(blocksCount)).ToHashSet();
ConsoleWritePath(path, blocks.Take(blocksCount));
Console.WriteLine($"Shortest path with {blocksCount} blocks: {path.Count - 1}");

// part 2
for (int i = blocksCount; i < blocks.Count; ++i)
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
            Console.WriteLine($"No paths with {blocksCount} blocks: blocked by {block}");
            break;
        }
    }
}

IEnumerable<Coord> FindShortestPath(Coord zStart, Coord zTarget, IEnumerable<Coord> blocks) => FindShortestPathBFS(zStart, zTarget, blocks);

IEnumerable<Coord> FindShortestPathBFS(Coord zStart, Coord zTarget, IEnumerable<Coord> blocks)
{
    HashSet<Coord> blocked = new(blocks);
    Dictionary<Coord, Coord> previous = new();
    Queue<Coord> queue = new([zStart]);
    while (queue.TryDequeue(out Coord zCurr))
    {
        if (blocked.Add(zCurr)) // aka visited (blocked doubles as visited set)
        {
            if (zCurr == zTarget)
            {
                // TODO search from zTarget to zStart to avoid reversing the path?
                return GenerateWhile(zTarget, previous.ContainsKey, z => previous[z]).Reverse();
            }

            IEnumerable<Coord> unvisitedNeighbours = GetNeighbours(zCurr)
                .Where(zn => !blocked.Contains(zn))
                .Where(zn => !previous.ContainsKey(zn));

            foreach (Coord zNext in unvisitedNeighbours)
            {
                previous.Add(zNext, zCurr);
                queue.Enqueue(zNext);
            }
        }
    }

    return Enumerable.Empty<Coord>(); // no path found
}

// Dijkstra is just a slower BFS if all path lengths are 1 :(
IEnumerable<Coord> FindShortestPathDijkstra(Coord zStart, Coord zTarget, IEnumerable<Coord> blocks)
{
    HashSet<Coord> blocked = new(blocks);
    Dictionary<Coord, Node> nodes = new() { [zStart] = new Node(zStart, 0) };
    PriorityQueue<Coord, int> queue = new([(zStart,0)]);
    while (queue.TryDequeue(out Coord z, out _))
    {
        blocked.Add(z); // aka visited (blocked doubles as visited set)
        Node node = nodes[z];

        if (z == zTarget)
        {
            // TODO search from zTarget to zStart to avoid reversing the path?
            return GenerateWhile(node, n => n.Prev != null, n => n.Prev!).Select(n => n.Z).Reverse();
        }

        IEnumerable<Coord> zNexts = GetNeighbours(z).Where(zn => !blocked.Contains(zn));
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

                queue.Remove(zNext, out _, out _);
                queue.Enqueue(zNext, stepsToNext);
            }
        }
    }

    return Enumerable.Empty<Coord>(); // no path found
}

IEnumerable<Coord> FindShortestPathRecursiveDFS(Coord zStart, Coord zTarget, IEnumerable<Coord> blocks)
{
    HashSet<Coord> blocked = new(blocks);
    Dictionary<Coord, int> stepCounts = new();

    // TODO: recursive DFS overflows after only ~1100 recursions (why are the stack frames so big?)
    IEnumerable<IReadOnlyCollection<Coord>> ExtendPathToTarget(IImmutableStack<Coord> path, int steps)
    {
        Coord z = path.Peek();

        if (!stepCounts.TryGetValue(z, out int previousSteps))
        {
            stepCounts.Add(z, steps);// first time here
        }
        else if (steps < previousSteps)
        {
            stepCounts[z] = steps; // got here quicker than before
        }
        else
        {
            return []; // got here slower than before, backtrack
        }

        if (z == zTarget)
        {
            // TODO search from zTarget to zStart to avoid reversing the path?
            return [path.Reverse().ToList()]; // return this path (reversed)
        }

        return GetNeighbours(z)
                .Where(zn => !blocked.Contains(zn))
                .OrderBy(GetDistanceTo(zTarget))
                .SelectMany(zn => ExtendPathToTarget(path.Push(zn), steps + 1))
                .WhereMinBy(path => path.Count);
    }

    var paths = ExtendPathToTarget(ImmutableStack.Create(zStart), 0).ToList();
    return paths.FirstOrDefault([]);
}

// TODO: iterative DFS implementation is slow especially with few blocks, can optimise? also ugly :/
IEnumerable<Coord> FindShortestPathIterativeDFS(Coord zStart, Coord zTarget, IEnumerable<Coord> blocks)
{
    HashSet<Coord> blocked = new(blocks);
    Dictionary<Coord, int> stepCounts = new();
    
    List<Candidate> results = new();
    Stack<Candidate> stack = new([Candidate.Create(zStart)]);
    while (stack.TryPop(out Candidate candidate))
    {
        Coord z = candidate.Path.Peek();
        int steps = candidate.Steps;

        if (!stepCounts.TryGetValue(z, out int previousSteps))
        {
            stepCounts.Add(z, steps); // first time here
        }
        else if (steps < previousSteps)
        {
            stepCounts[z] = steps; // got here quicker than before
        }
        else
        {
            continue; // got here slower than before, backtrack
        }

        if (z == zTarget)
        {
            if (results.Any() && results.First().Steps > steps)
            {
                results.Clear(); // drop worse candidates
            }

            results.Add(candidate); // add this path to best results so far
        }
        else
        {
            IEnumerable<Coord> neighbours = GetNeighbours(z)
                .Where(zn => !blocked.Contains(zn))
                .OrderByDescending(GetDistanceTo(zTarget)); // descending because order is reversed on the stack

            foreach (Coord zNext in neighbours)
            {
                stack.Push(candidate.AddStep(zNext));
            }
        }
    }

    Console.WriteLine($"Found paths: {results.Count}");
    return results.Select(c => c.Path).FirstOrDefault([]).Reverse();
}

IEnumerable<T> GenerateWhile<T>(T initial, Predicate<T> hasSucessor, Func<T, T> successor)
{
    yield return initial;
    for (T item = initial; hasSucessor(item); )
    {
        item = successor(item);
        yield return item;
    }
}

Func<Coord, int> GetDistanceTo(Coord zTarget) => z => GetDistance(z, zTarget);
static int GetDistance(Coord z1, Coord z2)
{
    Coord dz = z1 - z2;
    return Math.Abs(dz.X) + Math.Abs(dz.Y);
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

record struct Candidate(ImmutableStack<Coord> Path, int Steps)
{
    public static Candidate Create(Coord zStart) => new(ImmutableStack.Create(zStart), 0);
    public Candidate AddStep(Coord z) => new(Path.Push(z), Steps + 1);
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
