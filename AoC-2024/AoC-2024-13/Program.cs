using MoreLinq;
using System.Text.RegularExpressions;

Coord offset = (10000000000000L, 10000000000000L);

List<Input> inputs = new(
    File.ReadLines("input.txt")
    .Where(line => line.Trim().Length > 0)
    .Batch(3)
    .Select(l => new Input(
        ParseButton(l[0], "A"),
        ParseButton(l[1], "B"),
        ParsePrize(l[2])
    )));

// Part 1
List<(Input Input, Win Win)> results = inputs
    .SelectMany(FindWins, (input, win) => (input, win))
    .ToList();

foreach ((Input input, Win win) in results)
{
    Console.WriteLine($"A:{input.A}*{win.ACount} B:{input.B}*{win.BCount} = Prize:{input.Prize} @ Cost:{win.Cost}");
}
Console.WriteLine($"Total cost {results.Sum(r => r.Win.Cost)}\n");

// Part 2
List<(Input Input, Win Win)> results2 = inputs
    .Select(input => new Input(input.A, input.B, input.Prize + offset))
    .SelectMany(FindWins, (input, win) => (input, win))
    .ToList();

foreach ((Input input, Win win) in results2)
{
    Console.WriteLine($"A:{input.A}*{win.ACount} B:{input.B}*{win.BCount} = Prize:{input.Prize} @ Cost:{win.Cost}");
}
Console.WriteLine($"Total cost {results2.Sum(r => r.Win.Cost)}\n");

IEnumerable<Win> FindWins(Input input)
{
    // argggh... solve for (an,bn) in (an * A + bn * B == Prize)
    (Coord a, Coord b, Coord p) = (input.A, input.B, input.Prize);
    long an = (p.Y * b.X - p.X * b.Y) / (a.Y * b.X - a.X * b.Y);
    long bn = (p.X - an * a.X) / b.X;
    Coord z = an * a + bn * b;
    if (z == p)
    {
        yield return new Win(an, bn);
    }
}

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

record struct Input(Coord A, Coord B, Coord Prize);
record struct Win(long ACount, long BCount)
{
    public const int ACost = 3;
    public const int BCost = 1;
    public long Cost => ACount * ACost + BCount * BCost;
}

record struct Coord(long X, long Y)
{
    public static implicit operator Coord((long X, long Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new Coord(a.X - b.X, a.Y - b.Y);
    public static Coord operator *(Coord a, long f) => f * a;
    public static Coord operator *(long f, Coord a) => new Coord(f * a.X, f * a.Y);
    public override string ToString() => $"({X},{Y})";
}
