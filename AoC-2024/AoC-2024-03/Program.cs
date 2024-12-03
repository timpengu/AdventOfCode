using System.Text.RegularExpressions;

internal static class Program
{
    public static void Main(string[] args)
    {
        Parser parser = new();
        List<Parser.Result> results = File.ReadLines("input.txt")
            .SelectMany(parser.Parse)
            .ToList();

        int sumAll = results.Sum(r => r.X * r.Y);
        int sumEnabled = results.Where(r => r.Enabled).Sum(r => r.X * r.Y);

        foreach(var result in results)
        {
            Console.ForegroundColor = result.Enabled ? ConsoleColor.White : ConsoleColor.Red;
            Console.Write($"mul({result.X},{result.Y}) ");
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\n\n{sumAll} {sumEnabled}");
    }

    private class Parser
    {
        public record Result(int X, int Y, bool Enabled);
        public bool Enabled { get; private set; } = true;

        public IEnumerable<Result> Parse(string s)
        {
            var matches = Regex.Matches(s, @"mul\(([0-9]+),([0-9]+)\)|do\(\)|don\'t\(\)");

            foreach (Match match in matches)
            {
                if (match.Value == "do()")
                {
                    Enabled = true;
                }
                else if (match.Value == "don't()")
                {
                    Enabled = false;
                }
                else
                {
                    int x = int.Parse(match.Groups[1].Value);
                    int y = int.Parse(match.Groups[2].Value);
                    yield return new Result(x, y, Enabled);
                }
            }
        }
    }
}