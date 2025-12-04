
using System.Text;

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

var rolls = new HashSet<Coord>();
foreach (int x in Enumerable.Range(0, xs))
{
    foreach (int y in Enumerable.Range(0, ys))
    {
        if (lines[y][x] == '@')
        {
            rolls.Add((x,y));
        }
    }
}

int removedCount = 0;
ISet<Coord> moveable;
do
{
    moveable = rolls.GetMoveable().ToHashSet();

    Console.WriteLine($"Moveable {moveable.Count} of {rolls.Count}:");
    Console.WriteLine(Stringify(rolls, moveable));

    foreach (Coord z in moveable)
    {
        rolls.Remove(z);
        ++removedCount;
    }
}
while (moveable.Count > 0);

Console.WriteLine($"Total removed: {removedCount}");

string Stringify(ISet<Coord> rolls, ISet<Coord> moveable)
{
    var sb = new StringBuilder();
    foreach (int y in Enumerable.Range(0, ys))
    {
        foreach (int x in Enumerable.Range(0, xs))
        {
            sb.Append(
                moveable.Contains((x,y)) ? 'x' :
                rolls.Contains((x,y)) ? '@' : '.'
            );
        }
        sb.AppendLine();
    }
    return sb.ToString();
}

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public override string ToString() => $"({X},{Y})";
}

internal static class Extensions
{
    public static IEnumerable<Coord> GetMoveable(this ISet<Coord> rolls) =>
        rolls.Where(z => z.GetNeighbours(rolls).Count() < 4);

    public static IEnumerable<Coord> GetNeighbours(this Coord z, ISet<Coord> rolls) =>
        Directions.Select(dz => z + dz).Where(rolls.Contains);

    private static Coord[] Directions = { (0, 1), (1, 0), (0, -1), (-1, 0), (1, 1), (1, -1), (-1, -1), (-1, 1) };
}
