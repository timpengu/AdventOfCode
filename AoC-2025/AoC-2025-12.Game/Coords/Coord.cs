using System.Text.RegularExpressions;

namespace AoC_2025_12.GameModel.Coords;

public record struct Coord(int X, int Y) : IComparable<Coord>
{
    public static Coord Zero = new(0, 0); // default

    public static Coord MinValue = new(int.MinValue, int.MinValue);
    public static Coord MaxValue = new(int.MaxValue, int.MaxValue);

    public static implicit operator Coord((int X, int Y) value) => new Coord(value.X, value.Y);

    public static Coord operator +(Coord a, Coord b) => new(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new(a.X - b.X, a.Y - b.Y);
    public static Coord operator -(Coord a) => Zero - a;

    public static Coord Min(Coord a, Coord b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    public static Coord Max(Coord a, Coord b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

    public int CompareTo(Coord other)
    {
        int compareY = Y.CompareTo(other.Y);
        if (compareY != 0) return compareY;

        int compareX = X.CompareTo(other.X);
        if (compareX != 0) return compareX;

        return 0;
    }

    public static Coord Parse(string s) =>
        TryParse(s, out Coord coord)
        ? coord
        : throw new FormatException($"Invalid {nameof(Coord)} string '{s}'");

    public static bool TryParse(string s, out Coord coord) =>
        TryParseComponents(s, out coord) ||
        TryParseIndex(s, out coord);

    private static bool TryParseComponents(string s, out Coord coord)
    {
        s = s.Trim(); // Ignore leading and trailing whitespace

        if (s.Length > 0 && s[0] == '(' && s[^1] == ')')
        {
            s = s[1..^1]; // Ignore optional enclosing parentheses
        }

        string[] components = s.Split(',', 3);

        if (components.Length == 2 &&
            int.TryParse(components[0], out int x) &&
            int.TryParse(components[1], out int y))
        {
            coord = new Coord(x, y);
            return true;
        }

        coord = default;
        return false;
    }

    private static bool TryParseIndex(string s, out Coord coord)
    {
        s = s.Trim().ToUpper(); // Ignore leading/trailing whitespace and case

        // TODO: Support base-26 alphabetic y-components beyond row Z? 
        var match = Regex.Match(s, @"^([A-Z])([0-9]+)$");
        
        if (match.Success)
        {
            (string yComponent, string xComponent) = (match.Groups[1].Value, match.Groups[2].Value);

            if (int.TryParse(xComponent, out int xIndex))
            {
                int y = yComponent.Single() - 'A'; // A-based alphabetic index
                int x = xIndex - 1; // 1-based numeric index

                coord = new Coord(x, y);
                return true;
            }
        }

        coord = default;
        return false;
    }

    public override string ToString() => $"({X},{Y})";
}
