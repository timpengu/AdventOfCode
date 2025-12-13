namespace AoC_2025_12.GameModel.Coords;

public static class CoordExtensions
{
    private static Coord NoTransform(this Coord coord) => coord;

    public static Coord Rotate90(this Coord coord) => new(-coord.Y, coord.X);
    public static Coord Rotate180(this Coord coord) => new(-coord.X, -coord.Y);
    public static Coord Rotate270(this Coord coord) => new(coord.Y, -coord.X);
    public static Coord Reflect(this Coord coord) => new(-coord.X, coord.Y);
    public static Coord ReflectRotate90(this Coord coord) => new(coord.Y, coord.X);
    public static Coord ReflectRotate180(this Coord coord) => new(coord.X, -coord.Y);
    public static Coord ReflectRotate270(this Coord coord) => new(-coord.Y, -coord.X);

    public static Func<Coord, Coord> GetTransform(this Orientation orientation) => orientation switch
    {
        Orientation.Ar => NoTransform,
        Orientation.Br => Rotate90,
        Orientation.Cr => Rotate180,
        Orientation.Dr => Rotate270,
        Orientation.Al => Reflect,
        Orientation.Bl => ReflectRotate90,
        Orientation.Cl => ReflectRotate180,
        Orientation.Dl => ReflectRotate270,
        _ => throw new ArgumentException($"Invalid {nameof(Orientation)}: {orientation}", nameof(orientation))
    };

    public static IEnumerable<Coord> Transform(this IEnumerable<Coord> coords, Orientation orientation) => coords.Select(orientation.GetTransform());
    public static IEnumerable<Coord> Transpose(this IEnumerable<Coord> coords, Coord offset) => coords.Select(coord => coord + offset);

    public static IEnumerable<Coord> Normalise(this IEnumerable<Coord> coords)
    {
        List<Coord> positionsList = coords.ToList(); // copy for multiple enumeration
        CoordRange bounds = positionsList.GetBounds();
        return positionsList
            .Transpose(-bounds.Start) // transpose to origin
            .Order(); // apply standard ordering
    }

    public static CoordRange GetBounds(this IEnumerable<Coord> coords)
        => coords
            .ThrowIfEmpty()
            .Aggregate(
                new CoordRange(Coord.MaxValue, Coord.MinValue),
                (range, coord) => new CoordRange(
                    Coord.Min(range.Start, coord),
                    Coord.Max(range.End, coord + new Coord(1, 1)) // use exclusive range end
                ));
}