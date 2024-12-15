using System.Diagnostics;
using System.Text.RegularExpressions;

internal static class Program
{
    private static readonly int verbosity = 1;

    const string InputMapRegex = @"^[#.O@]+$";
    const string InputDirRegex = @"^[<^>v]+$";

    public static void Main(string[] args)
    {
        List<string> mapLines = new();
        List<Direction> directions = new();
        foreach (string line in File.ReadLines("input.txt").Where(l => l.Length > 0))
        {
            if (Regex.IsMatch(line, InputMapRegex))
            {
                mapLines.Add(line);
            }
            else if (Regex.IsMatch(line, InputDirRegex))
            {
                directions.AddRange(line.Select(Direction.Parse));
            }
            else throw new FormatException($"Cannot parse input: '{line}'");
        }

        (Map map1, Coord zRobot1) = BuildMap(mapLines);
        int blockSum1 = Run(map1, zRobot1, 1, directions);
        Console.WriteLine($"Block position sum (single): {blockSum1}\n");

        var mapLinesDouble = mapLines.Select(ToDoubleWidth).ToList();
        (Map map2, Coord zRobot2) = BuildMap(mapLinesDouble);
        int blockSum2 = Run(map2, zRobot2, 2, directions);
        Console.WriteLine($"Block position sum (double): {blockSum2}\n");
    }

    private static int Run(Map map, Coord zRobot, int blockWidth, IList<Direction> directions)
    {
        if (verbosity >= 1)
        {
            Console.WriteLine("\nInitial state");
            map.ConsoleWriteMap(zRobot);
        }

        // TODO: support both block widths in a single function?
        Func<Coord, bool> tryMove = blockWidth switch
        {
            1 => dz => map.TryMoveSingle(ref zRobot, dz),
            2 => dz => map.TryMoveDouble(ref zRobot, dz),
            _ => throw new NotSupportedException($"Invalid width: {blockWidth}")
        };

        int moves = 0;
        foreach (var direction in directions)
        {
            if (!tryMove(direction.Vector))
            {
                if (verbosity >= 2)
                {
                    Console.WriteLine($"Cannot move {direction.Name}");
                }
                continue;
            }

            ++moves;

            if (verbosity >= 2)
            {
                Console.WriteLine($"Move {direction.Name}");
            }

            if (verbosity >= 3)
            {
                map.ConsoleWriteMap(zRobot);
            }
        }

        if (verbosity >= 1 && verbosity < 3)
        {
            Console.WriteLine($"\nFinal state after {moves}/{directions.Count} moves");
            map.ConsoleWriteMap(zRobot);
        }

        return map.GetBlockPositionSum();
    }

    private static bool TryMoveSingle(this Map map, ref Coord z, Coord dz)
    {
        // find the next space or wall (skip past any blocks)
        Coord zNext = z + dz;
        while (map[zNext] == Glyphs.Block)
        {
            zNext += dz;
        }

        // cannot move if there is a wall (beyond any blocks)
        if (map[zNext] == Glyphs.Wall)
        {
            return false;
        }

        Debug.Assert(map[zNext] == Glyphs.Space);

        // move the robot forward
        z += dz;

        // swap the block previously here (if any) into the next free space
        map[zNext] = map[z];
        map[z] = Glyphs.Space;

        return true;
    }

    private static bool TryMoveDouble(this Map map, ref Coord z, Coord dz)
    {
        Coord? zPushBlock = null;

        Coord zNext = z + dz;
        switch (map[zNext])
        {
            case Glyphs.Wall:
                // cannot move into a wall
                return false;

            case Glyphs.Space:
                // move forward into the space
                break;

            case Glyphs.LeftBlock:
                // try pushing the block (on its left side)
                zPushBlock = zNext;
                break;

            case Glyphs.RightBlock:
                // try pushing the block (on its right side)
                zPushBlock = zNext - (1, 0);
                break;

            default:
                throw new NotSupportedException($"Cannot push invalid item: '{map[zNext]}'");
        }

        if (zPushBlock.HasValue)
        {
            // get moveable blocks in the order they must be moved
            var moveableBlocks = GetMoveableBlocks(map, zPushBlock.Value, dz).ToList();
            if (moveableBlocks.Count == 0)
            {
                // cannot push the block/s
                return false;
            }

            // move the stack of blocks
            foreach (var block in moveableBlocks)
            {
                MoveDoubleBlock(map, block, dz);
            }
        }

        // move the robot forward
        z += dz;

        return true;
    }

    private static void MoveDoubleBlock(Map map, Coord zBlock, Coord dz)
    {
        Coord dx = (1, 0); // vector to the adjacent block half

        Debug.Assert( // zBlock must be a block!
            map[zBlock] == Glyphs.LeftBlock &&
            map[zBlock + dx] == Glyphs.RightBlock);

        // remove the block from its current position
        map[zBlock] = Glyphs.Space;
        map[zBlock + dx] = Glyphs.Space;

        Debug.Assert( // new position must (now) be a space!
            map[zBlock + dz] == Glyphs.Space &&
            map[zBlock + dz + dx] == Glyphs.Space);

        // place the block in its new position
        map[zBlock + dz] = Glyphs.LeftBlock;
        map[zBlock + dz + dx] = Glyphs.RightBlock;
    }

    // TODO: support single width blocks?
    private static IEnumerable<Coord> GetMoveableBlocks(this Map map, Coord zBlock, Coord dz) =>
        (dz.X != 0 && dz.Y == 0) ? GetMoveableBlocksHorizontal(map, zBlock, dz) :
        (dz.X == 0 && dz.Y != 0) ? GetMoveableBlocksVertical(map, zBlock, dz) :
            throw new NotSupportedException($"Invalid direction: {dz}");

    private static IEnumerable<Coord> GetMoveableBlocksHorizontal(this Map map, Coord zBlock, Coord dz)
    {
        Coord dx = (1, 0); // vector to the adjacent block half
        Debug.Assert( // zBlock must be a block!
            map[zBlock] == Glyphs.LeftBlock &&
            map[zBlock + dx] == Glyphs.RightBlock);

        Stack<Coord> zPushBlocks = new();

        // find the next space or wall (skip past any blocks)
        Coord zNext = zBlock;
        while (map[zNext] is Glyphs.LeftBlock or Glyphs.RightBlock)
        {
            if (map[zNext] == Glyphs.LeftBlock)
            {
                zPushBlocks.Push(zNext); // this block has to move before the previous block/s
            }

            zNext += dz;
        }

        if (map[zNext] == Glyphs.Wall)
        {
            return []; // cannot push blocks through a wall
        }

        Debug.Assert(map[zNext] == Glyphs.Space);

        return zPushBlocks; // can push this row of blocks into the space
    }

    private static IEnumerable<Coord> GetMoveableBlocksVertical(this Map map, Coord zBlock, Coord dz)
    {
        Coord dx = (1, 0); // vector to the adjacent block half
        Debug.Assert( // zBlock must be a block!
            map[zBlock] == Glyphs.LeftBlock &&
            map[zBlock + dx] == Glyphs.RightBlock);

        // Coords of next blocks this block might push on
        Coord zNextC = zBlock + dz;
        Coord zNextL = zBlock + dz - dx;
        Coord zNextR = zBlock + dz + dx;

        char c1 = map[zNextC];
        char c2 = map[zNextR];
        
        if (c1 == Glyphs.Wall || c2 == Glyphs.Wall)
        {
            return []; // cannot push this block through a wall
        }

        if (c1 == Glyphs.Space && c2 == Glyphs.Space)
        {
            return [zBlock]; // can move this block without pushing on any others
        }

        // This block is pushing on some other block/s
        Coord[] zNexts = [zNextL, zNextC, zNextR];
        List<Coord> zPushBlocks = zNexts.Where(z => map[z] == Glyphs.LeftBlock).ToList();

        Debug.Assert(zPushBlocks.Count > 0 && zPushBlocks.Count <= 2);
        Debug.Assert(zPushBlocks.All(z => map[z] == Glyphs.LeftBlock && map[z + dx] == Glyphs.RightBlock));

        return zPushBlocks
            .Select(zPushBlock => GetMoveableBlocksVertical(map, zPushBlock, dz)) // get all moveable pushed blocks recursively
            .Append([zBlock]) // append the current block
            .ConcatIfNoneEmpty() // return all pushed blocks if they are moveable
            .Distinct(); // avoid duplicates in case of diamond dependencies
    }

    private static int GetBlockPositionSum(this Map map) =>
        map.Range()
            .Where(z => map[z] is Glyphs.Block or Glyphs.LeftBlock)
            .Sum(z => z.X + z.Y * 100);

    private static void ConsoleWriteMap(this Map map, Coord zRobot)
    {
        foreach (int y in Enumerable.Range(0, map.YSize))
        {
            foreach (int x in Enumerable.Range(0, map.XSize))
            {
                Coord z = (x, y);
                char c = (z == zRobot) ? Glyphs.Robot : map[z];
                Console.ForegroundColor = c switch {
                    Glyphs.Wall => ConsoleColor.Red,
                    Glyphs.Robot => ConsoleColor.Green,
                    Glyphs.Space => ConsoleColor.DarkGray,
                    _ => ConsoleColor.DarkYellow
                };
                Console.Write(c);
            }
            Console.ForegroundColor= ConsoleColor.White;
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private static string ToDoubleWidth(this string line) => string.Concat(
        line.SelectMany(c => c switch
        {
            Glyphs.Robot => [Glyphs.Robot, Glyphs.Space],
            Glyphs.Block => [Glyphs.LeftBlock, Glyphs.RightBlock],
            _ => Enumerable.Repeat(c, 2)
        }));

    private static (Map Map, Coord ZRobot) BuildMap(IList<string> lines)
    {
        int xs = lines.Select(x => x.Length).Distinct().Single();
        int ys = lines.Count;

        char[,] map = new char[xs, ys];
        Coord? zRobot = null;

        for (int y = 0; y < ys; y++)
        {
            for (int x = 0; x < xs; x++)
            {
                char c = lines[y][x];
                if (c == Glyphs.Robot)
                {
                    zRobot = (x, y);
                    c = Glyphs.Space;
                }

                map[x, y] = c;
            }
        }

        return (
            new Map(map),
            zRobot ?? throw new InvalidDataException("Input contains no robot")
        );
    }
}
