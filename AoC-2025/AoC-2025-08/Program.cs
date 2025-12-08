
List<Coord> nodes = File.ReadLines("input.txt")
    .Select(s => s.Split(",", 3, StringSplitOptions.TrimEntries).Select(int.Parse).ToArray())
    .Select(a => new Coord(a[0], a[1], a[2]))
    .ToList();

int part1Connections = 1000;

var closestNodes = (
    from ai in Enumerable.Range(0, nodes.Count)
    from bi in Enumerable.Range(ai + 1, nodes.Count - (ai + 1))
    let a = nodes[ai]
    let b = nodes[bi]
    orderby Distance(a, b)
    select (a, b)
);

List<HashSet<Coord>> circuits = [];
foreach (var (a,b) in closestNodes.Take(part1Connections))
{
    circuits.Connect(a, b);
}

Console.WriteLine("\nPart1 circuits:");
foreach(var circuit in circuits.OrderByDescending(g => g.Count))
{
    Console.WriteLine($"{String.Join(",", circuit)} => {circuit.Count}");
}

long part1Product = circuits.Select(g => g.Count).OrderByDescending(c => c).Take(3).Product();
Console.WriteLine($"\nPart1 product: {part1Product}\n");

foreach (var (a, b) in closestNodes.Skip(part1Connections))
{
    circuits.Connect(a, b);
    
    Console.WriteLine($"+ {a}-{b} => {circuits.Count} groups");

    if (circuits.Count == 1 && circuits[0].Count == nodes.Count)
    {
        Console.WriteLine($"\nFinal connection: {a}-{b}");
        Console.WriteLine($"Final X product: {(long)a.X * b.X}");
        break;
    }
}

static double Distance(Coord a, Coord b) => (b - a).Magnitude;

record struct Coord(int X, int Y, int Z)
{
    public static implicit operator Coord((int X, int Y, int Z) tuple) => new Coord(tuple.X, tuple.Y, tuple.Z);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public double Magnitude => Math.Sqrt(((long) X * X) + ((long) Y * Y) + ((long) Z * Z));
    public override string ToString() => $"({X},{Y},{Z})";
}

internal static class Extensions
{
    public static void Connect(this List<HashSet<Coord>> groups, Coord a, Coord b)
    {
        var aIndex = groups.FindIndex(group => group.Contains(a));
        var bIndex = groups.FindIndex(group => group.Contains(b));

        if (aIndex < 0 && bIndex < 0)
        {
            //Console.WriteLine($"+ {a}-{b}");
            groups.Add(new HashSet<Coord>([a, b]));
        }
        else if (aIndex >= 0 && bIndex < 0)
        {
            //Console.WriteLine($"{String.Join("-", groups[aIndex])} + {b}");
            groups[aIndex].Add(b);
        }
        else if (aIndex < 0 && bIndex >= 0)
        {
            //Console.WriteLine($"{String.Join("-", groups[bIndex])} + {a}");
            groups[bIndex].Add(a);
        }
        else if (aIndex != bIndex)
        {
            //Console.WriteLine($"{String.Join("-", groups[aIndex])} + {String.Join("-", groups[bIndex])}");
            int minIndex = Math.Min(aIndex, bIndex);
            int maxIndex = Math.Max(aIndex, bIndex);
            foreach (var z in groups[maxIndex])
            {
                groups[minIndex].Add(z);
            }
            groups.RemoveAt(maxIndex);
        }
    }

    public static long Product(this IEnumerable<int> values) => values.Aggregate(1L, (a, b) => a * b);
}
