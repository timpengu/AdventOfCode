using IntCode;

internal static class Program
{
    private static void Main(string[] args)
    {
        List<long> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToList();

        // Part 1
        var painted1 = program.RunPainter(Colour.Black);
        Console.WriteLine($"Painted panels: {painted1.Count}\n");

        // Part 2
        var painted2 = program.RunPainter(Colour.White);
        Visualise(painted2);
    }

    public static IReadOnlyDictionary<Coord, Colour> RunPainter(this IEnumerable<long> program, Colour startColour)
    {
        Coord z = (0, 0);
        Coord dz = (0, -1);

        Dictionary<Coord, Colour> painted = new() { [z] = startColour };
        long GetInput() => (long)(painted.TryGetValue(z, out var colour) ? colour : Colour.Black);

        Computer<long> computer = new(program, GetInput);
        while (true)
        {
            long[] outputs = computer.ExecuteOutputs().Take(2).ToArray();
            if (computer.IsHalted)
                break;

            var colour = outputs[0].ToColour();
            dz = outputs[1].ToNewDirection(dz);

            Console.WriteLine($"{z,10} {dz.ToDirectionChar()} {colour}");

            painted[z] = colour;
            z += dz;
        }

        Console.WriteLine();
        return painted;
    }

    public static void Visualise(IReadOnlyDictionary<Coord, Colour> painted)
    {
        int xMin = painted.Keys.Min(z => z.X);
        int xMax = painted.Keys.Max(z => z.X);
        int yMin = painted.Keys.Min(z => z.Y);
        int yMax = painted.Keys.Max(z => z.Y);

        for (int y = yMin; y <= yMax; ++y)
        {
            for (int x = xMin; x <= xMax; ++x)
            {
                Coord z = (x, y);
                char c = painted.TryGetValue(z, out var colour) && colour == Colour.White ? '#' : '.';
                Console.Write(c);
            }
            Console.WriteLine();
        }
    }

    public static char ToDirectionChar(this Coord dz) => dz switch
    {
        (1,0) => '>',
        (0,1) => 'v',
        (-1,0) => '<',
        (0,-1) => '^',
        _ => throw new Exception($"Invalid direction: {dz}")
    };

    public static Colour ToColour(this long output) => output switch
    {
        0 => Colour.Black,
        1 => Colour.White,
        _ => throw new Exception($"Invalid colour output: {output}")
    };

    public static Coord ToNewDirection(this long output, Coord dz) => output switch
    {
        0 => dz.RotateLeft(),
        1 => dz.RotateRight(),
        _ => throw new Exception($"Invalid direction output: {output}")
    };
}

enum Colour
{
    Black = 0,
    White = 1,
}

