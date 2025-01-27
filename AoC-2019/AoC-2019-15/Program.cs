using IntCode;

internal static class Program
{
    private static void Main(string[] args)
    {
        List<long> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToList();

        List<Direction> path = program.FindShortestPath();
        Console.WriteLine($"Shortest path [{path.Count}]: {String.Concat(path.Select(d => d.Char))}");

        // TODO: explore entire region and return map instead of shortest path
        // TODO: add map visualisation
        // TODO: run SPF on map instead of Computer
        // TODO: return SPF from oxygen to origin (part 1)
        // TODO: return maximum path from oxygen (part 2)
    }

    private static List<Direction> FindShortestPath(this IReadOnlyCollection<long> program)
    {
        Dictionary<Coord, Direction> pathDirections = new();
        HashSet<Coord> visited = new();
        Queue<Coord> queue = new([(0,0)]);
        while (queue.TryDequeue(out Coord zCurr))
        {
            if (visited.Add(zCurr))
            {
                IReadOnlyCollection<Direction> pathToCurr = pathDirections.GetPathTo(zCurr);

                foreach(Direction direction in Direction.All)
                {
                    Coord zNext = zCurr + direction.dZ;
                    if (visited.Contains(zNext) || pathDirections.ContainsKey(zNext))
                    {
                        // already visited zNext or found a shorther path
                        continue;
                    }

                    // execute the path and get the output for the next step
                    IEnumerable<Direction> pathToNext = pathToCurr.Append(direction);
                    long[] inputs = pathToNext.Select(d => d.Input).ToArray();
                    Computer<long> computer = new(program, inputs);
                    long nextOutput = computer.ExecuteOutputs().Skip(pathToCurr.Count).First();

                    switch(nextOutput)
                    {
                        case 0: // can't move in this direction
                            break;

                        case 1: // moved in this direction
                            pathDirections.Add(zNext, direction);
                            queue.Enqueue(zNext);
                            break;

                        case 2: // found the target by the shortest path
                            return pathToNext.ToList();

                        default:
                            throw new Exception($"Invalid output: {nextOutput}");
                    }
                }
            }
        }

        return []; // no path found
    }

    private static IReadOnlyCollection<Direction> GetPathTo(
        this IReadOnlyDictionary<Coord, Direction> pathDirection, Coord z)
    {
        Stack<Direction> directions = new();
        while (pathDirection.TryGetValue(z, out Direction direction))
        {
            z -= direction.dZ; // move to previous coord
            directions.Push(direction);
        }
        return directions;
    }

    private record struct Direction(char Char, long Input, Coord dZ)
    {
        public static readonly Direction North = new('N', 1, (0, -1));
        public static readonly Direction South = new('S', 2, (0, +1));
        public static readonly Direction West  = new('W', 3, (-1, 0));
        public static readonly Direction East  = new('E', 4, (+1, 0));

        public static readonly Direction[] All =
        {
            Direction.North,
            Direction.South,
            Direction.West,
            Direction.East
        };
    }
}
