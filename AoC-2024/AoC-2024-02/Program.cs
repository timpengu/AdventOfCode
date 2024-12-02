using MoreLinq;

internal static class Program
{
    static void Main(string[] args)
    {
        List<List<int>> ls = new(
            File.ReadLines("input.txt")
                .Select(line => line
                    .Split(default(char[]), StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList()
        ));

        int safeCount0 = ls.Count(l => l.IsSafe());
        
        int safeCount1 = ls.Count(l =>
            l.IsSafe() ||
            Enumerable.Range(0, l.Count).Any(index => l.SkipAt(index).IsSafe()));

        Console.WriteLine($"{safeCount0} {safeCount1}");
    }

    static bool IsSafe(this IEnumerable<int> source) =>
        source.Pairwise((a, b) => b - a).All(d => d is >= +1 and <= +3) ||
        source.Pairwise((a, b) => b - a).All(d => d is <= -1 and >= -3);

    static IEnumerable<int> SkipAt(this IEnumerable<int> source, int index) =>
        source.Where((_,i) => i != index);
}