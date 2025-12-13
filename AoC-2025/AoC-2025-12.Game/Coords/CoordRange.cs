namespace AoC_2025_12.GameModel.Coords;

/// <summary>
/// Defines a bounding box with half-open coordinate ranges
/// </summary>
/// <param name="Start">Start of bounding coordinate range (inclusive)</param>
/// <param name="End">End of bounding coordinate range (exclusve)</param>
public record struct CoordRange(Coord Start, Coord End)
{
    public IEnumerable<Coord> EnumerateCoords()
    {
        foreach (int y in EnumerateY())
        {
            foreach (int x in EnumerateX())
            {
                yield return new Coord(x, y);
            }
        }
    }

    public IEnumerable<int> EnumerateX() => Range(Start.X, End.X);
    public IEnumerable<int> EnumerateY() => Range(Start.Y, End.Y);
    private static IEnumerable<int> Range(int start, int end)
    {
        if (start > end)
            throw new InvalidOperationException($"Negative range: [{start}..{end})");

        return Enumerable.Range(start, end - start);
    }

    public static CoordRange Parse(string s) =>
        TryParse(s, out CoordRange range)
        ? range
        : throw new FormatException($"Invalid {nameof(CoordRange)} string '{s}'");

    public static bool TryParse(string s, out CoordRange range)
    {
        s = s.Trim(); // Ignore leading and trailing whitespace

        if (s.Length > 0 && s[0] == '[' && s[^1] == ']')
        {
            s = s[1..^1]; // Ignore optional enclosing parentheses
        }

        string[] components = s.Split("..", 3);

        if (components.Length == 2 &&
            Coord.TryParse(components[0], out Coord start) &&
            Coord.TryParse(components[1], out Coord end))
        {
            range = new CoordRange(start, end);
            return true;
        }

        range = default;
        return false;
    }

    public override string ToString() => $"[{Start}..{End}]";
}
