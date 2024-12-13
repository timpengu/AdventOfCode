using MoreLinq;
using System.Text.RegularExpressions;

const int aCost = 3;
const int bCost = 1;

Coord offset = (10000000000000L, 10000000000000L);

List<(Coord A, Coord B, Coord Prize)> input = new(File.ReadLines("input.txt")
    .Where(line => line.Trim().Length > 0)
    .Batch(3)
    .Select(l => (
        A: ParseButton(l[0], "A"),
        B: ParseButton(l[1], "B"),
        Prize: ParsePrize(l[2])
    ))
);

long totalCost = 0;

foreach(var item in input)
{
    List<Win> wins = FindWins2(item.A, item.B, item.Prize + offset).ToList();
    foreach(var win in wins)
    {
        Console.WriteLine($"A:{item.A}*{win.A} B:{item.B}*{win.B} = Prize:{item.Prize + offset} @ Cost:{GetCost(win)}");
    }
    if (wins.Count > 0)
    {
        totalCost += wins.Min(GetCost);
    }
}

Console.WriteLine($"Total cost {totalCost}");

IEnumerable<Win> FindWins1(Coord a, Coord b, Coord prize)
{
    for (long aCount = 0; aCount <= GetMaxCount(a, prize); ++aCount)
    {
        Coord remainder = prize - aCount * a;
        if (remainder.TryDiv(b, out long bCount))
        {
            yield return new Win(aCount, bCount);
        }
    }
}

IEnumerable<Win> FindWins2(Coord a, Coord b, Coord prize)
{
    // ohhhh shit oh fuck I mean solve for (an,bn) in: an * a + bn * b == prize
    long pd = prize.Y * b.X - prize.X * b.Y;
    long ad = a.Y * b.X - a.X * b.Y;
    long an = pd / ad;
    long bn = (prize.X - an * a.X) / b.X;
    Coord z = an * a + bn * b;
    if (z == prize)
    {
        yield return new Win(an, bn);
    }
}

long GetCost(Win move) => move.A * aCost + move.B * bCost;
long GetMaxCount(Coord z, Coord prize) => Min(prize.X / z.X, prize.Y / z.Y);
long Min(params long[] values) => values.Min();

Coord ParseButton(string line, string name)
{
    Match match = Regex.Match(line, @"^Button ([A-Za-z]+): X\+([0-9]+), Y\+([0-9]+)$");
    if (match.Success &&
        match.Groups[1].Value == name &&
        long.TryParse(match.Groups[2].Value, out long x) &&
        long.TryParse(match.Groups[3].Value, out long y))
    {
        return (x, y);
    }

    throw new FormatException($"Failed to parse line: '{line}'");
}

Coord ParsePrize(string line)
{
    Match match = Regex.Match(line, @"^Prize: X=([0-9]+), Y=([0-9]+)$");
    if (match.Success &&
        long.TryParse(match.Groups[1].Value, out long x) &&
        long.TryParse(match.Groups[2].Value, out long y))
    {
        return (x, y);
    }

    throw new FormatException($"Failed to parse line: '{line}'");
}

record struct Win(long A, long B);
record struct Coord(long X, long Y)
{
    public static implicit operator Coord((long X, long Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new Coord(a.X - b.X, a.Y - b.Y);
    public static Coord operator *(Coord a, long f) => f * a;
    public static Coord operator *(long f, Coord a) => new Coord(f * a.X, f * a.Y);

    public bool TryDiv(Coord divisor, out long result)
    {
        if (X / divisor.X == Y / divisor.Y &&
            X % divisor.X == 0 &&
            Y % divisor.Y == 0)
        {
            result = X / divisor.X;
            return true;
        }

        result = 0;
        return false;
    }

    public override string ToString() => $"({X},{Y})";
}
