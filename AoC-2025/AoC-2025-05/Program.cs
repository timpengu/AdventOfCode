
var items = new List<long>();
var ranges = new List<(long First, long Last)>();

var lines = File.ReadLines("input.txt")
    .Select(line => line.Trim().Split('-', 2, StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray())
    .Where(line => line.Any());

foreach (long[] line in lines)
{
    if (line.Length > 1)
    {
        ranges.Add((line[0], line[1]));
    }
    else
    {
        items.Add(line[0]);
    }
}

int itemsInRange = 0;
foreach(long i in items)
{
    bool isFresh = ranges.Any(r => i >= r.First && i <= r.Last);
    itemsInRange += isFresh ? 1 : 0;
    Console.WriteLine($"{i} : {(isFresh ? "fresh" : "spoiled")}");
}

Console.WriteLine();

long totalInRange = ranges.Coalesce2().Sum(c => c.Last - c.First + 1);

Console.WriteLine();
Console.WriteLine($"Items in range: {itemsInRange}");
Console.WriteLine($"Total in range: {totalInRange}");

static class Extensions
{
    public static IEnumerable<(long First, long Last)> Coalesce1(this IEnumerable<(long First, long Last)> ranges)
    {
        List<(long First, long Last)> merged = [];
        foreach (var r in ranges)
        {
            int i = merged.FindLastIndex(m => m.First <= r.First); // range comes after i (or overlaps)
            int j = merged.FindIndex(m => m.Last >= r.Last); // range comes before j (or overlaps)

            bool iOverlap = i >= 0 && merged[i].Last >= r.First; // range overlaps i?
            bool jOverlap = j >= 0 && merged[j].First <= r.Last; // range overlaps j?

            long first = iOverlap ? merged[i].First : r.First; // first of new range
            long last = jOverlap ? merged[j].Last : r.Last; // last of new range

            merged.RemoveInclusiveRange(iOverlap ? i : i + 1, jOverlap ? j : j - 1); // remove overlapping ranges
            merged.Insert(iOverlap ? i : i + 1, (first, last)); // insert new range

            Console.WriteLine($"+{r.First}-{r.Last}: {String.Join(',', merged.Select(c => $"{c.First}-{c.Last}"))}");
        }
        return merged;
    }

    public static IEnumerable<(long First, long Last)> Coalesce2(this IEnumerable<(long First, long Last)> ranges)
    {
        var orderedRanges = ranges.OrderBy(r => r.First).GetEnumerator();
        if (!orderedRanges.MoveNext()) yield break;

        (long First, long Last) range = orderedRanges.Current;
        Console.WriteLine($"{range.First}-{range.Last}");

        while (orderedRanges.MoveNext())
        {
            var nextRange = orderedRanges.Current;
            if (nextRange.First > range.Last)
            {
                yield return range;
                range = nextRange;
                Console.WriteLine($"{range.First}-{range.Last}");
            }
            else if (nextRange.Last > range.Last)
            {
                range = (range.First, nextRange.Last);
                Console.WriteLine($"+{nextRange.First}-{nextRange.Last} => {range.First}-{range.Last}");
            }
            else // nextRange subsumed by range
            {
                Console.WriteLine($"({nextRange.First}-{nextRange.Last})");
            }
        }
        yield return range;
    }

    public static void RemoveInclusiveRange<T>(this List<T> l, int first, int last)
    {
        int count = last - first + 1;
        if (count > 0)
        {
            l.RemoveRange(first, count);
        }
    }
}