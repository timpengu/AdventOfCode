
using System.Text.RegularExpressions;

List<(long First, long Last)> ranges = File.ReadLines("input.txt")
    .SelectMany(line => line.Split(","))
    .Select(s => s.Trim())
    .Where(s => s.Length > 0)
    .Select(item => 
    {
        long[] values = item.Trim().Split("-", 2).Select(long.Parse).ToArray();
        return (values[0], values[1]);
    })
    .ToList();

long sum1 = 0;
long sum2 = 0;

foreach (var range in ranges)
{
    List<long> invalid1 = range.Enumerate().Where(IsInvalid1).ToList();
    List<long> invalid2 = range.Enumerate().Where(IsInvalid2).ToList();

    Console.WriteLine($"{range.First}-{range.Last} ({String.Join(',', invalid1)}) ({String.Join(',', invalid2)})");

    sum1 += invalid1.Sum();
    sum2 += invalid2.Sum();
}

Console.WriteLine($"\n{sum1}\n{sum2}");

static bool IsInvalid1(long value) => Regex.IsMatch(value.ToString(), @"^(\d+)\1{1}$", RegexOptions.Compiled);
static bool IsInvalid2(long value) => Regex.IsMatch(value.ToString(), @"^(\d+)\1{1,}$", RegexOptions.Compiled);

static class Extensions
{
    public static IEnumerable<long> Enumerate(this (long First, long Last) range)
    {
        for (long x = range.First; x <= range.Last; x++)
        {
            yield return x;
        }
    }
}
