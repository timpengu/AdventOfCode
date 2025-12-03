
List<int[]> lines = File.ReadLines("input.txt")
    .Select(line => line.Select(c => c.ToDigit()).ToArray())
    .ToList();

long sum2 = 0;
long sum12 = 0;

foreach (var line in lines)
{
    long max2 = line.MaxJolts(2);
    sum2 += max2;

    long max12 = line.MaxJolts(12);
    sum12 += max12;

    Console.WriteLine($"{String.Concat(line)} max2={max2} max12={max12}");
}

Console.WriteLine($"sum2={sum2}");
Console.WriteLine($"sum12={sum12}");

internal static class Extensions
{
    public static int ToDigit(this char c) =>
        c >= '0' && c <= '9'
        ? c - '0'
        : throw new ArgumentOutOfRangeException($"Invalid digit: {c}");

    public static long MaxJolts(this int[] values, int digits)
    {
        if (digits < 1 || digits > values.Length)
            throw new ArgumentOutOfRangeException(nameof(digits));
        
        long jolts = 0;
        int indexStart = 0;
        for (int digit = 0; digit < digits; digit++)
        {
            int indexEnd = values.Length - digits + digit;
            int indexMax = InclusiveRange(indexStart, indexEnd).MaxBy(i => values[i]);
            jolts = jolts * 10 + values[indexMax];
            indexStart = indexMax + 1;
        }

        return jolts;
    }

    private static IEnumerable<int> InclusiveRange(int start, int end)
    {
        return start <= end
            ? Enumerable.Range(start, end - start + 1)
            : throw new ArgumentOutOfRangeException($"Invalid range: {start}..{end}");
    }
}
