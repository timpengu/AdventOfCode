using MoreLinq;
using System.Diagnostics;

internal static class Program
{
    const int maxDisplayValues = 1000;

    private static Dictionary<(ulong Value, int Expansions), long> memoizedCounts = new();

    private static void Main(string[] args)
    {
        List<ulong> values =
            File.ReadLines("input.txt")
            .Single()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(ulong.Parse)
            .ToList();

        Console.WriteLine("Initial arrangement:");
        Console.WriteLine(string.Join(' ', values));

        for (int blinks = 1; blinks <= 75; ++blinks)
        {
            Console.WriteLine($"\nAfter blink {blinks}:");

            long count = CountExpanded(values, blinks);

            if (blinks <= 25)
            {
                List<ulong> expandedValues = values.Expand(blinks).ToList();
                Debug.Assert(expandedValues.Count == count);

                Console.Write(String.Join(' ', expandedValues.Take(maxDisplayValues)));
                Console.WriteLine(expandedValues.Count > maxDisplayValues ? "..." : "");

                Console.WriteLine($"Log10 distribution: {string.Join(' ', expandedValues.GetDigitsFrequency())}");
            }

            Console.WriteLine($"Memoized counts: {memoizedCounts.Count}");
            Console.WriteLine($"Number of stones: {count} ({count:e0})");
        }
    }

    static long CountExpanded(this IEnumerable<ulong> values, int expansions) => values.Sum(value => value.CountExpanded(expansions));
    static long CountExpanded(this ulong value, int expansions)
    {
        if (expansions == 0)
        {
            return 1;
        }

        if (!memoizedCounts.TryGetValue((value, expansions), out long count))
        {
            count = value.Expand().CountExpanded(expansions - 1);
            memoizedCounts.Add((value, expansions), count);
        }

        return count;
    }

    static IEnumerable<ulong> Expand(this IEnumerable<ulong> values, int expansions) => values.SelectMany(value => value.Expand(expansions));
    static IEnumerable<ulong> Expand(this ulong value, int expansions)
    {
        return expansions == 0
            ? [value]
            : value.Expand().Expand(expansions - 1);
    }

    static IEnumerable<ulong> Expand(this ulong value)
    {
        if (value == 0)
        {
            yield return 1;
        }
        else if (value.TrySplit(out ulong a, out ulong b))
        {
            yield return a;
            yield return b;
        }
        else
        {
            yield return value * 2024;
        }
    }

    static bool TrySplit(this ulong value, out ulong a, out ulong b)
    {
        uint d = value.GetDigits();
        if (d % 2 == 0)
        {
            ulong mod = (ulong)Math.Pow(10, d / 2);
            a = value / mod;
            b = value % mod;
            return true;
        }
        else
        {
            a = 0;
            b = 0;
            return false;
        }
    }

    static uint GetDigits(this ulong value) => value switch
    {
        < 10L => 1,
        < 100L => 2,
        _ => (uint)Math.Log10(value) + 1
    };

    static IEnumerable<int> GetDigitsFrequency(this IEnumerable<ulong> values)
    {
        Dictionary<int, int> digitsFrequency = values.CountBy(GetDigits).ToDictionary(kvp => (int)kvp.Key, kvp => kvp.Value);
        return Enumerable.Range(1, digitsFrequency.Keys.Max())
            .Select(digits => digitsFrequency.TryGetValue(digits, out int freq) ? freq : 0)
            .ToList();
    }
}