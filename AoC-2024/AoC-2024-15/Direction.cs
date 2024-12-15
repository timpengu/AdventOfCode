record struct Direction(Char Glyph, string Name, Coord Vector)
{
    public static Direction Right = new(Glyphs.Right, "right", (+1, 0));
    public static Direction Down  = new(Glyphs.Down,  "down",  (0, +1));
    public static Direction Left  = new(Glyphs.Left,  "left",  (-1, 0));
    public static Direction Up    = new(Glyphs.Up,    "up",    (0, -1));

    private static Direction[] _allDirections = [Right, Down, Left, Up];
    private static Dictionary<char, Direction> _directions = _allDirections.ToDictionary(d => d.Glyph);

    public static Direction Parse(char glyph) =>
        _directions.TryGetValue(glyph, out var direction)
        ? direction
        : throw new FormatException($"Unknown direction: {glyph}");
}
