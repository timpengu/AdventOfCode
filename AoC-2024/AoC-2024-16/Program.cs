using MoreLinq;

Coord InitialDirection = (+1, 0); // start facing east

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

(IList<Coord> zs, Coord zStart, Coord zEnd) = Parse(lines);
IList<Coord> path = FindShortestPath(zs, zStart, zEnd);
(int steps, int turns, int score) = CalcPathMetrics(path);

Console.WriteLine($"Score:{score} Steps:{steps} Turns:{turns} Path:{String.Join("", path)}");

(int Steps, int Turns, int Score) CalcPathMetrics(IList<Coord> path)
{
    int steps = path.Count - 1;
    int turns = path
        .Pairwise((z1, z2) => z2 - z1)
        .Prepend(InitialDirection)
        .Pairwise((dz1, dz2) => (dz1, dz2))
        .Count(step => step.dz1 != step.dz2);

    return (steps, turns, steps + turns * 1000);
}

IList<Coord> FindShortestPath(IEnumerable<Coord> zs, Coord zStart, Coord zEnd)
{
    Dictionary<Coord, Node> nodes = zs.ToDictionary(z => z, z => new Node(Cost:int.MaxValue));
    nodes[zStart] = new Node(Cost:0);
    
    HashSet<Coord> unvisited = nodes.Keys.ToHashSet();
    while (unvisited.Count > 0)
    {
        Coord z = unvisited.MinBy(z => nodes[z].Cost);
        Node node = nodes[z];

        // Console.WriteLine($"{z} => Cost:{zNode.Cost} Facing:{zNode.Facing} Prev:{zNode.Previous}");

        if (z == zEnd)
        {
            Console.WriteLine($"Found target with cost {node.Cost}");

            var path = new Stack<Coord>();
            Coord? zPath = z;
            while (zPath.HasValue)
            {
                path.Push(zPath.Value);
                zPath = nodes[zPath.Value].zPrev;
            }

            ConsoleWritePaths(nodes, path);
            return path.ToList();
        }

        var neighbours = GetDirections(z, node).Select(dz => z + dz).Where(unvisited.Contains);
        foreach (Coord zNext in neighbours)
        {
            Coord dzPrev = GetFacing(z, node);
            Coord dzNext = zNext - z;
            int cost = node.Cost + 1 + (dzNext == dzPrev ? 0 : 1000);

            if (cost < nodes[zNext].Cost)
            {
                nodes[zNext] = new Node(cost, z);
            }
        }

        unvisited.Remove(z);
    }

    throw new Exception("Cannot find any path");
}

IEnumerable<Coord> GetDirections(Coord z, Node node)
{
    Coord dz = GetFacing(z, node);
    yield return dz; // straight ahead
    yield return (-dz.Y, dz.X); // turn left
    yield return (dz.Y, -dz.X); // turn right
}

Coord GetFacing(Coord z, Node node) => z - node.zPrev ?? InitialDirection;

void ConsoleWritePaths(IDictionary<Coord, Node> nodes, IEnumerable<Coord> path)
{
    var pathSet = path.ToHashSet();
    foreach (int y in Enumerable.Range(0, ys))
    {
        foreach (int x in Enumerable.Range(0, xs))
        {
            Coord z = (x, y);

            if (!nodes.TryGetValue(z, out Node node))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write('#');
                continue;
            }

            Console.ForegroundColor = pathSet.Contains(z) ? ConsoleColor.Green : ConsoleColor.DarkRed;
            Console.Write(
                (z - node.zPrev ?? (0, 0)) switch
                {
                    (+1, 0) => '>',
                    (0, +1) => 'v',
                    (-1, 0) => '<',
                    (0, -1) => '^',
                    _ => ' '
                });
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }
    Console.WriteLine();

}

(IList<Coord>, Coord, Coord) Parse(IList<string> lines)
{
    List<Coord> nodes = new();
    Coord? start = null, end = null;
    foreach (int x in Enumerable.Range(0, xs))
    {
        foreach (int y in Enumerable.Range(0, ys))
        {
            char c = lines[y][x];
            if (c == '#')
                continue; // ignore walls

            nodes.Add((x, y));

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
        nodes,
        start ?? throw new InvalidDataException("No start position"),
        end ?? throw new InvalidDataException("No end position")
    );
}

record struct Node(int Cost, Coord? zPrev = null);
record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new Coord(a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X},{Y})";
}
