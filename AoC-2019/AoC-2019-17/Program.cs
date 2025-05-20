using IntCode;

internal static class Program
{
    private static class Ascii
    {
        public const char Null = '\0';
        public const char Scaffold = '#';
    }

    public static void Main(string[] args)
    {
        List<long> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(long.Parse)
            .ToList();

        Part1(program);
        Part2(program);
    }

    private static void Part1(IEnumerable<long> program)
    {
        var computer = new Computer<long>(program);

        IEnumerable<char> output = computer.ExecuteOutputs().Select(ToChar);
        string[] lines = String.Concat(output).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Coord size = (lines.Max(l => l.Length), lines.Length);

        bool IsInRange(Coord z) => z.X >= 0 && z.X < size.X && z.Y >= 0 && z.Y < size.Y;
        char GetChar(Coord z) => IsInRange(z) ? lines[z.Y][z.X] : Ascii.Null;
        bool IsScaffold(Coord z) => GetChar(z) == Ascii.Scaffold;

        Coord[] IntersectOffsets = [(0, 0), (0, 1), (1, 0), (-1, 0), (0, -1)];
        bool IsIntersect(Coord z) => IntersectOffsets.All(dz => IsScaffold(z + dz));

        List<Coord> intersects = new(
            from x in Enumerable.Range(0, size.X)
            from y in Enumerable.Range(0, size.Y)
            let z = new Coord(x, y)
            where IsIntersect(z)
            select z
        );

        Console.WriteLine($"Intersections: {String.Join(' ', intersects)}");
        Console.WriteLine($"Alignment sum: {intersects.Sum(z => z.X * z.Y)}");
    }

    private static void Part2(IEnumerable<long> program)
    {
        var inputQueue = new BlockingInputQueue<long>();
        var computer = new Computer<long>(program, inputQueue);

        computer[0] = 2; // wake the robot 🤖

        // Read input from Console in a background task (thread)
        Task.Run(() =>
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                foreach (char input in line.Append('\n'))
                {
                    inputQueue.Enqueue(input);
                }
            }
        });

        // Execute program and write output to Console
        Console.WriteLine();
        foreach (long output in computer.ExecuteOutputs())
        {
            Console.Write(output.IsChar() ? output.ToChar() : $"Output: {output}\n");
        }
    }

    private static bool IsChar(this long output) => output is >= Char.MinValue and <= Char.MaxValue;
    private static char ToChar(this long output) => IsChar(output) ? (char) output : throw new Exception($"Invalid char: {output}");
}
