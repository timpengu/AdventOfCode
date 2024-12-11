using MoreLinq;

List<ulong> values =
    File.ReadLines("input.txt")
    .Single()
    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
    .Select(ulong.Parse)
    .ToList();

IEnumerable<ulong> Expand(IEnumerable<ulong> values)
{
    foreach (ulong value in values)
    {
        if (value == 0)
        {
            yield return 1;
        }
        else if (TrySplit(value, out ulong a, out ulong b))
        {
            yield return a;
            yield return b;
        }
        else
        {
            yield return value * 2024;
        }
    }
}

bool TrySplit(ulong value, out ulong a, out ulong b)
{
    uint d = GetDigits(value);
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

uint GetDigits(ulong value) => value switch
{
    < 10L => 1,
    < 100L => 2,
    _ => (uint)Math.Log10(value) + 1
};

Console.WriteLine("Initial arrangement:");
Console.WriteLine(String.Join(' ', values));

List<List<ulong>> allValues = [values];
for (int blink = 1; blink <= 75; ++blink)
{
    allValues = allValues
        .AsParallel()
        .SelectMany(l => l)
        .Batch(100000)
        .Select(l => Expand(l).ToList())
        .ToList();

    Console.WriteLine($"\nAfter blink {blink}:");
    // Console.WriteLine(String.Join(' ', allValues.SelectMany(l => l)));
    Console.WriteLine($"Number of stones: {allValues.Sum(l => l.Count)} (in {allValues.Count} lists)");
}
