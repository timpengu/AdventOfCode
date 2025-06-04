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
        List<int> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        Part1(program);
        Part2(program);
    }

    private static void Part1(IEnumerable<int> program)
    {
        var computer = new Computer<int>(program);

        IEnumerable<char> output = computer.ExecuteOutputs().Select(ToChar);
        string[] lines = String.Concat(output).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        List<Coord> intersects = GetIntersections(lines);

        Console.WriteLine($"Intersections: {String.Join(' ', intersects)}");
        Console.WriteLine($"Alignment sum: {intersects.Sum(z => z.X * z.Y)}");
    }

    private static List<Coord> GetIntersections(string[] lines)
    {
        Coord size = (lines.Max(l => l.Length), lines.Length);
        Coord[] IntersectOffsets = [(0, 0), (0, 1), (1, 0), (-1, 0), (0, -1)];

        bool IsInRange(Coord z) => z.X >= 0 && z.X < size.X && z.Y >= 0 && z.Y < size.Y;
        char GetChar(Coord z) => IsInRange(z) ? lines[z.Y][z.X] : Ascii.Null;
        bool IsScaffold(Coord z) => GetChar(z) == Ascii.Scaffold;
        bool IsIntersect(Coord z) => IntersectOffsets.All(dz => IsScaffold(z + dz));

        return new List<Coord>(
            from x in Enumerable.Range(0, size.X)
            from y in Enumerable.Range(0, size.Y)
            let z = new Coord(x, y)
            where IsIntersect(z)
            select z
        );
    }

    private static void Part2(IEnumerable<int> program)
    {
        var inputs = new BlockingInputQueue<int>();
        var computer = new Computer<int>(program, inputs);

        computer[0] = 2; // wake the robot 🤖

        // Read input from Console in a background task (thread)
        Task.Run(() =>
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                foreach (char input in line.Append('\n'))
                {
                    inputs.Enqueue(input);
                }
            }
        });

        // Execute program and write output to Console
        Console.WriteLine();
        foreach (int output in computer.ExecuteOutputs())
        {
            Console.Write(output.IsChar() ? output.ToChar() : $"Output: {output}\n");
        }
    }

    private static bool IsChar(this int output) => output is >= Char.MinValue and <= Char.MaxValue;
    private static char ToChar(this int output) => IsChar(output) ? (char) output : throw new Exception($"Invalid char: {output}");
}
