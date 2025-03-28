using IntCode;
using System.Diagnostics;

internal static class Program
{
    private static void Main(string[] args)
    {
        List<long> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToList();

        Coord zOrigin = (0, 0);
        IReadOnlyDictionary<Coord, Block> map = program.GenerateMap(zOrigin);
        Visualise(map);

        var zOxygen = map.Single(m => m.Value == Block.Oxygen).Key;
        (int distOxygen, int distMax) = GetDistances(map, zOxygen, zOrigin);

        Console.WriteLine($"Shortest distance to oxygen: {distOxygen}");
        Console.WriteLine($"Maximum distance from oxygen: {distMax}");
    }

    private static IReadOnlyDictionary<Coord,Block> GenerateMap(this IReadOnlyCollection<long> program, Coord zStart)
    {
        Dictionary<Coord, Block> map = new() { [zStart] = Block.Space };
        Dictionary<Coord, Direction> pathDirections = new();
        HashSet<Coord> visited = new();
        Queue<Coord> queue = new([zStart]);
        while (queue.TryDequeue(out Coord zCurr))
        {
            if (!visited.Add(zCurr))
            {
                continue;
            }

            IReadOnlyCollection<Direction> pathToCurr = pathDirections.GetPathTo(zCurr);

            foreach (Direction direction in Direction.All)
            {
                Coord zNext = zCurr + direction.dZ;

                if (visited.Contains(zNext) || pathDirections.ContainsKey(zNext))
                {
                    // already visited zNext or found a shorther path
                    continue;
                }

                // TODO: refactor the Computer to the outer loop and push each direction to explore onto the queue
                // execute the path and get the output for the next step
                IEnumerable<Direction> pathToNext = pathToCurr.Append(direction);
                long[] inputs = pathToNext.Select(d => d.Input).ToArray();
                Computer<long> computer = new(program, inputs);
                long nextOutput = computer.ExecuteOutputs().Skip(pathToCurr.Count).First();

                Block block = nextOutput.ToBlock();

                Debug.Assert(!map.TryGetValue(zNext, out Block blockExisting) || block == blockExisting);
                map[zNext] = block;

                if (block != Block.Wall) // can move in this direction?
                {
                    pathDirections.Add(zNext, direction);
                    queue.Enqueue(zNext);
                }
            }
        }

        return map;
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

    private static (int TargetDistance, int MaxDistance) GetDistances(IReadOnlyDictionary<Coord, Block> map, Coord zStart, Coord zTarget)
    {
        HashSet<Coord> path = map
            .Where(m => m.Value != Block.Wall)
            .Select(m => m.Key)
            .ToHashSet();

        Dictionary<Coord, int> dist = new() { [zStart] = 0 };
        Queue<Coord> queue = new([zStart]);

        while (queue.TryDequeue(out Coord zCurr))
        {
            if (path.Remove(zCurr)) // doubles as unvisited set
            {
                int distCurr = dist[zCurr];

                IEnumerable<Coord> unvisitedNeighbours =
                    Direction.All
                    .Select(dir => zCurr + dir.dZ)
                    .Where(zn => path.Contains(zn))
                    .Where(zn => !dist.ContainsKey(zn));

                foreach (Coord zNext in unvisitedNeighbours)
                {
                    dist.Add(zNext, distCurr + 1);
                    queue.Enqueue(zNext);
                }
            }
        }

        int distTarget = dist[zTarget];
        int distMax = dist.Values.Max();

        return (distTarget, distMax);
    }

    private static void Visualise(IReadOnlyDictionary<Coord, Block> map)
    {
        var xMin = map.Keys.Min(z => z.X);
        var yMin = map.Keys.Min(z => z.Y);
        var xMax = map.Keys.Max(z => z.X);
        var yMax = map.Keys.Max(z => z.Y);
        for (int y = yMin; y <= yMax; ++y)
        {
            for (int x = xMin; x <= xMax; ++x)
            {
                Coord z = (x, y);
                Block block = map.TryGetValue(z, out var b) ? b : Block.Unknown;
                char c = block switch
                {
                    Block.Wall => '#',
                    Block.Space => '.',
                    Block.Oxygen => 'O',
                    Block.Unknown => ' ',
                    _ => throw new Exception($"Invalid block: {block}")
                };
                if (z == (0,0))
                {
                    c = 'D';
                }
                Console.Write(c);
            }
            Console.WriteLine();
        }
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

    private static Block ToBlock(this long output)
    {
        var block = (Block)output;
        return Enum.IsDefined<Block>(block) ? block
            : throw new Exception($"Invalid block: {block}");
    }

    private enum Block
    {
        Unknown = -1,
        Wall = 0,
        Space = 1,
        Oxygen = 2
    }
}
