using IntCode;

internal static class Program
{
    public static void Main(string[] args)
    {
        List<int> program = string.Concat(File.ReadLines("input.txt"))
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var inputs = new BlockingInputQueue<int>();
        var computer = new Computer<int>(program, inputs);

        // Read input from Console in a background task (thread)
        Task.Run(() =>
        {
            string? line;
            while ((line = Console.ReadLine()) != null)
            {
                line = line.Split('#', 2, StringSplitOptions.TrimEntries).First();
                if (String.IsNullOrWhiteSpace(line)) continue;
                
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
    private static char ToChar(this int output) => IsChar(output) ? (char)output : throw new Exception($"Invalid char: {output}");
}
