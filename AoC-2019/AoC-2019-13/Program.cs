using IntCode;
using MoreLinq;

internal static class Program
{
    private const int HeaderLines = 4;
    private enum Tile
    {
        Empty = 0,
        Wall = 1,
        Block = 2,
        Paddle = 3,
        Ball = 4
    }

    public static void Main(string[] args)
    {
        List<long> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToList();

        int? xBall = null;
        int? xPaddle = null;

        long GetInput() =>
            xBall.HasValue && xPaddle.HasValue
            ? xBall.Value.CompareTo(xPaddle.Value)
            : 0;

        var computer = new Computer<long>(program, GetInput);
        computer[0] = 2; // insert coins to play :)

        IEnumerable<(Coord Z, long Value)> outputs = computer
            .ExecuteOutputs()
            .Batch(3)
            .Select(output =>
            {
                Coord z = new((int)output[0], (int)output[1]);
                return (z, output[2]);
            });

        Dictionary<Coord, Tile> tiles = new();
        int score = 0;
        bool isStarted = false;

        foreach (var output in outputs)
        {
            Coord z = output.Z;
            if (z == (-1,0)) // is this a score update?
            {
                score = (int)output.Value;
                DisplayHeader(tiles, score);
                
                if (!isStarted)
                {
                    isStarted = true;
                    foreach (int countdown in MoreEnumerable.Sequence(5,0))
                    {
                        DisplayMessage(countdown > 0 ? $"Ready in {countdown}..." : "GO!");
                        Thread.Sleep(500);
                    }
                }
                continue;
            }

            Tile tile = (Tile)output.Value;
            tiles[z] = tile;

            DisplayUpdate(z, tile);
            DisplayTile(z, tile);

            if (tile == Tile.Ball)
            {
                xBall = z.X;
            }
            if (tile == Tile.Paddle)
            {
                xPaddle = z.X;
                Thread.Sleep(1); // sleep to refresh console after paddle updated
            }
        }

        DisplayMessage("Game over!");
        DisplayHeader(tiles, score);
        DisplayGame(tiles, score);
    }

    private static void DisplayHeader(IReadOnlyDictionary<Coord, Tile> tiles, int score)
    {
        int blockCount = tiles.Values.Count(t => t == Tile.Block);

        Console.SetCursorPosition(0, 0);
        Console.WriteLine($"Score:  {score,-10}");
        Console.WriteLine($"Blocks: {blockCount,-10}");
    }

    private static void DisplayUpdate(Coord z, Tile tile) => DisplayMessage($"{z} => {tile}");
    private static void DisplayMessage(string message)
    {
        Console.SetCursorPosition(0, 2);
        Console.WriteLine($"{message,-40}");
    }

    private static void DisplayTile(Coord z, Tile tile)
    {
        Console.SetCursorPosition(z.X, HeaderLines + z.Y);
        Console.ForegroundColor = tile.ToColor();
        Console.Write(tile.ToChar());
        Console.ResetColor();
        Console.SetCursorPosition(0, HeaderLines - 1);
    }

    private static void DisplayGame(IReadOnlyDictionary<Coord, Tile> tiles, int score)
    {
        Console.SetCursorPosition(0, HeaderLines);

        int xMax = tiles.Keys.Max(z => z.X);
        int yMax = tiles.Keys.Max(z => z.Y);
        for (int y = 0; y <= yMax; ++y)
        {
            for (int x = 0; x <= xMax; ++x)
            {
                Coord z = (x, y);
                Tile tile = tiles.TryGetValue(z, out var t) ? t : Tile.Empty;
                Console.ForegroundColor = tile.ToColor();
                Console.Write(tile.ToChar());
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    private static char ToChar(this Tile tile) => tile switch
    {
        Tile.Empty => ' ',
        Tile.Wall => '█',
        Tile.Block => '#',
        Tile.Paddle => '▀',
        Tile.Ball => 'o',
        _ => throw new Exception($"Unknown tile: {tile}")
    };

    private static ConsoleColor ToColor(this Tile tile) => tile switch
    {
        Tile.Wall => ConsoleColor.Cyan,
        Tile.Block => ConsoleColor.Yellow,
        Tile.Ball => ConsoleColor.Red,
        _ => ConsoleColor.White,
    };
}

