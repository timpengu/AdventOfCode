public class Pattern: IImage
{
    public Pattern(IEnumerable<Coord> requiredCoords)
    {
        RequiredCoords = requiredCoords.ToHashSet();
        Size = (
            RequiredCoords.Max(z => z.X + 1),
            RequiredCoords.Max(z => z.Y + 1));
    }

    public IEnumerable<Coord> FindMatchingPositions(IImage image) =>
        from x in Enumerable.Range(0, image.Size.X - Size.X + 1)
        from y in Enumerable.Range(0, image.Size.Y - Size.Y + 1)
        let zOffset = new Coord(x, y)
        where RequiredCoords.Select(z => z + zOffset).All(z => image[z])
        select zOffset;

    public IEnumerable<Coord> GetMatchingCoords(IEnumerable<Coord> matchingPositions) =>
        from zOffset in matchingPositions
        from zPattern in RequiredCoords
        select zOffset + zPattern;

    public readonly ISet<Coord> RequiredCoords;
    public Coord Size { get; }
    public bool this[Coord z] => RequiredCoords.Contains(z);
}
