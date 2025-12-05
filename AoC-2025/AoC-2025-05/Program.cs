
var items = new List<long>();
var ranges = new List<(long First, long Last)>();

var lines = File.ReadLines("input.txt").Select(line => line.Trim()).Where(line => line.Length > 0);
foreach (string line in lines)
{
    int sep = line.IndexOf('-');
    if (sep >= 0)
    {
        long first = long.Parse(line[..sep]);
        long last = long.Parse(line[(sep+1)..]);
        ranges.Add((first, last));
    }
    else
    {
        long item = long.Parse(line);
        items.Add(item);
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

var merged = new List<(long First, long Last)>(ranges.Count);
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

    Console.WriteLine($"Adding {r.First}-{r.Last}: {String.Join(',', merged.Select(c => $"{c.First}-{c.Last}"))}");
}

long totalInRange = merged.Sum(c => c.Last - c.First + 1);

Console.WriteLine();
Console.WriteLine($"Items in range: {itemsInRange}");
Console.WriteLine($"Total in range: {totalInRange}");

static class Extensions
{
    public static void RemoveInclusiveRange<T>(this List<T> l, int first, int last)
    {
        int count = last - first + 1;
        if (count > 0)
        {
            l.RemoveRange(first, count);
        }
    }
}