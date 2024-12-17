using MoreLinq;

const int StepCost = 1;
const int TurnCost = 1000;

Coord InitialDirection = (+1, 0); // start facing east
Coord[] Directions = [(+1, 0), (0, +1), (-1, 0), (0, -1)];

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

(IList<Coord> zs, Coord zStart, Coord zEnd) = Parse(lines);

// part 1
IList<State> path = FindShortestPaths(zs, zStart, zEnd, false);
(int steps, int turns, int score) = CalcPathMetrics(path);
Console.WriteLine($"Score:{score} (Steps:{steps} Turns:{turns})\n");

// part 2
IList<State> paths = FindShortestPaths(zs, zStart, zEnd, true);
int zCount = paths.Select(p => p.Z).Distinct().Count();
Console.WriteLine($"Positions on shortest paths: {zCount}\n");

(int Steps, int Turns, int Score) CalcPathMetrics(IList<State> path)
{
    int steps = path.Select(s => s.Z).Distinct().Count() - 1;
    int turns = path.Select(s => s.dZ).Pairwise((dz1, dz2) => (dz1, dz2)).Count(s => s.dz1 != s.dz2);
    return (steps, turns, steps * StepCost + turns * TurnCost);
}

IList<State> FindShortestPaths(IEnumerable<Coord> zs, Coord zStart, Coord zTarget, bool includeAll)
{
    // build a graph of traversable states
    HashSet<Coord> coords = zs.ToHashSet();
    Dictionary<State, Node> nodes = new(
        from z in coords
        from dz in Directions
        where (coords.Contains(z - dz) && z != zStart) || (coords.Contains(z + dz) && z != zEnd) // can leave or enter in this direction
        let state = new State(z, dz)
        select KeyValuePair.Create(state, new Node(state))
    );

    State initialState = (zStart, InitialDirection);
    nodes[initialState] = new Node(initialState) { Cost = 0 };

    // Dijkstra to find shortest paths:
    HashSet<State> unvisited = new(nodes.Keys);
    PriorityQueue<State, int> pq = new([(initialState, 0)]);
    while (pq.TryDequeue(out State state, out _))
    {
        unvisited.Remove(state);

        Node node = nodes[state];

        foreach (var move in GetMoves(state).Where(move => unvisited.Contains(move.State)))
        {
            Node nextNode = nodes[move.State];

            int cost = node.Cost + move.Cost;
            if (cost < nextNode.Cost)
            {
                nextNode.Prev = [node]; // this is now the shortest path
                nextNode.Cost = cost;

                // update priority
                pq.Remove(move.State, out _, out _);
                pq.Enqueue(move.State, cost);
            }
            else if (cost == nextNode.Cost && includeAll)
            {
                nextNode.Prev.Add(node); // add this to the shortest paths
            }    
        }
    }

    List<State> paths = new(
        GetMinStates(nodes, zTarget) // there may be multiple shortest paths to the target from different directions
        .SelectMany(targetState => GetPaths(nodes, targetState))
    );

    // TODO: change Node.Prev to Node.Next and solve in reverse?
    paths.Reverse(); // list path/s from start to end

    ConsoleWritePaths(nodes, paths);

    return paths;
}

IEnumerable<(State State, int Cost)> GetMoves(State s)
{
    yield return ((s.Z + s.dZ, s.dZ), StepCost); // move forward
    yield return ((s.Z, s.dZ.RotateLeft()), TurnCost); // turn left
    yield return ((s.Z, s.dZ.RotateRight()), TurnCost); // turn right
}

IEnumerable<State> GetMinStates(IReadOnlyDictionary<State, Node> nodes, Coord zTarget) =>
    Directions
    .Select(dz => new State(zTarget, dz))
    .Where(nodes.ContainsKey)
    .Select(state => nodes[state])
    .WhereMinBy(node => node.Cost)
    .Select(node => node.State);

IEnumerable<State> GetPaths(IReadOnlyDictionary<State, Node> nodes, State target)
{
    HashSet<State> visited = new();
    Queue<State> toVisit = new([target]);

    while (toVisit.TryDequeue(out State state))
    {
        if (visited.Add(state))
        {
            yield return state;

            var prevStates = nodes[state].Prev.Select(p => p.State);
            foreach (var prevState in prevStates)
            {
                toVisit.Enqueue(prevState);
            }
        }
    }
}

void ConsoleWritePaths(IDictionary<State, Node> nodes, IEnumerable<State> paths)
{
    ISet<Coord> pathsLookup = paths.Select(s => s.Z).ToHashSet();
    ILookup<Coord, Node> nodesLookup = nodes.ToLookup(n => n.Key.Z, n => n.Value);

    IEnumerable<Coord> GetPrevs(IEnumerable<Node> nodes) => nodes
        .WhereMinBy(node => node.Cost)
        .SelectMany(node => node.Prev)
        .Select(prevNode => prevNode.State.Z)
        .Distinct();

    Dictionary<Coord, List<Coord>> minCostNodes = nodes
        .GroupBy(n => n.Key.Z, n => n.Value)
        .ToDictionary(g => g.Key, g => GetPrevs(g).ToList());

    foreach (int y in Enumerable.Range(0, ys))
    {
        foreach (int x in Enumerable.Range(0, xs))
        {
            Coord z = (x, y);

            if (!minCostNodes.TryGetValue(z, out List<Coord>? zPrevs))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write('#');
                continue;
            }

            Console.ForegroundColor = pathsLookup.Contains(z) ? ConsoleColor.Green : ConsoleColor.DarkRed;

            Console.Write(zPrevs.Count switch
            {
                > 1 => '@',
                0 => '.',
                _ => (z - zPrevs.Single()) switch
                {
                    (+1, 0) => '>',
                    (0, +1) => 'v',
                    (-1, 0) => '<',
                    (0, -1) => '^',
                    _ => '?'
                }
            });
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }
    Console.WriteLine();

}

(IList<Coord>, Coord, Coord) Parse(IList<string> lines)
{
    List<Coord> coords = new();
    Coord? start = null, end = null;
    foreach (int x in Enumerable.Range(0, xs))
    {
        foreach (int y in Enumerable.Range(0, ys))
        {
            char c = lines[y][x];
            if (c == '#')
                continue; // ignore walls

            coords.Add((x, y));

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
        coords,
        start ?? throw new InvalidDataException("No start position"),
        end ?? throw new InvalidDataException("No end position")
    );
}
