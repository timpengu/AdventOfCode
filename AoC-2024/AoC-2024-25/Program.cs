
using System.Diagnostics;

internal static class Program
{
    const int LockHeight = 5;
    const int InputHeight = LockHeight + 2;

    static void Main(string[] args)
    {
        List<int[]> Locks = new();
        List<int[]> Keys = new();

        using (StreamReader file = new("input.txt"))
        {
            while (!file.EndOfStream)
            {
                List<string> lines = new();
                for (string? line = file.ReadLine(); line?.Length > 0; line = file.ReadLine())
                {
                    lines.Add(line.Trim());
                }

                Debug.Assert(lines.Count == InputHeight);

                int length = lines.Select(p => p.Length).Distinct().Single();
                int[] heights = Enumerable.Range(0, length)
                    .Select(i => Enumerable.Range(0, lines.Count)
                        .TakeWhile(j => lines[j][i] == lines[0][i])
                        .Count() - 1)
                    .ToArray();

                if (lines[0].All(c => c == '#') && lines[^1].All(c => c == '.'))
                {
                    Locks.Add(heights);
                }
                else if (lines[0].All(c => c == '.') && lines[^1].All(c => c == '#'))
                {
                    Keys.Add(heights.InvertHeights());
                }
                else throw new Exception("Invalid pattern");
            }
        }

        Console.WriteLine("Locks:");
        foreach (int[] lok in Locks)
        {
            Console.WriteLine(string.Join(',', lok));
        }

        Console.WriteLine("\nKeys:");
        foreach (int[] key in Keys)
        {
            Console.WriteLine($"{string.Join(',', key)} [{string.Join(',', key.InvertHeights())}]");
        }

        List<(int[] Lock, int[] Key)> matches = new(
            from lok in Locks
            from key in Keys
            where key.IsFit(lok)
            select (lok, key)
        );

        Console.WriteLine("\nMatches:");
        foreach (var match in matches)
        {
            Console.WriteLine($"Lock:{string.Join(',', match.Lock)} Key:{string.Join(',', match.Key)}");
        }

        Console.WriteLine($"\nMatches: {matches.Count}\n");
    }

    static bool IsFit(this IEnumerable<int> seq1, IEnumerable<int> seq2) => seq1.Zip(seq2, (s1, s2) => s1 + s2).All(sf => sf <= LockHeight);
    static int[] InvertHeights(this IEnumerable<int> sequence) => sequence.Select(h => LockHeight - h).ToArray();
}
