using MoreLinq;
using System.Collections;

internal static class Program
{
    private static void Main(string[] args)
    {
        IList<ulong> seeds = File.ReadLines("input.txt").Select(ulong.Parse).ToList();

        int iterations = 2000;

        // Part 1
        ulong total = 0;
        foreach(ulong seed in seeds)
        {
            ulong value = GenerateIteration(seed, iterations);
            total += value;
            Console.WriteLine($"{seed}: {value}");
        }
        Console.WriteLine($"\nTotal of {seeds.Count} values after {iterations} iterations: {total}\n");

        // Part 2
        (ChangeSeq bestChangeSeq, int bestPrice) = seeds
            .SelectMany(seed =>
                GeneratePriceChanges(seed)
                .Take(iterations)
                .GetChangeSeqPrices() // get every overlapping change sequence and the price at its end 
                .GroupBy(v => v.Changes, v => v.Price)
                .Select(g => (Changes: g.Key, Price: g.First())) // take the first instance of each distinct change sequence
            )
            .GroupBy(v => v.Changes, v => v.Price)
            .Select(g => (g.Key, TotalPrice: g.Sum())) // sum the prices from every seed for each change sequence
            .OrderByDescending(g => g.TotalPrice)
            .First(); // take the best price

        Console.WriteLine($"Change sequence [{string.Join(',', bestChangeSeq)}] has total price: {bestPrice}");
    }

    private static IEnumerable<(ChangeSeq Changes, int Price)> GetChangeSeqPrices(this IEnumerable<(int Price, int Change)> source)
    {
        List<(int Price, int Change)> s = source.ToList();
        return from i in MoreEnumerable.Sequence(3, s.Count - 1)
               let price = s[i].Price
               let changeSeq = new ChangeSeq(s[i - 3].Change, s[i - 2].Change, s[i - 1].Change, s[i].Change)
               select (changeSeq, price);
    }

    private static IEnumerable<(int Price, int Change)> GeneratePriceChanges(ulong seed) => GeneratePrices(seed).Pairwise((v1, v2) => (v2, v2 - v1));
    private static IEnumerable<int> GeneratePrices(ulong seed) => Generate(seed).Select(value => (int)(value % 10));

    private static ulong GenerateIteration(ulong seed, int iteration) => Generate(seed).Skip(iteration - 1).First();
    private static IEnumerable<ulong> Generate(ulong seed) => MoreEnumerable.Generate(seed, GenerateNext);
    private static ulong GenerateNext(ulong value)
    {
        value = Mix(value, value << 6);
        value = Mix(value, value >> 5);
        value = Mix(value, value << 11);
        return value;
    }

    private static ulong Mix(ulong a, ulong b) => (a ^ b) % 16777216;

    private record struct ChangeSeq(int d1, int d2, int d3, int d4) : IEnumerable<int>
    {
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<int> GetEnumerator()
        {
            yield return d1;
            yield return d2;
            yield return d3;
            yield return d4;
        }
    }
}
