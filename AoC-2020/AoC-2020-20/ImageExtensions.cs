using MoreLinq;

public static class ImageExtensions
{
    public static Coord Max(this IImage image) => (image.MaxX(), image.MaxY());
    public static int MaxX(this IImage image) => image.Size.X - 1;
    public static int MaxY(this IImage image) => image.Size.Y - 1;

    public static IEnumerable<int> RangeX(this IImage image) => Enumerable.Range(0, image.Size.X);
    public static IEnumerable<int> RangeY(this IImage image) => Enumerable.Range(0, image.Size.Y);
    public static IEnumerable<Coord> Range(this IImage image) =>
        from y in image.RangeY()
        from x in image.RangeX()
        select new Coord(x, y);

    public static IEnumerable<Edge> GetEdges(this IImage image) =>
        Enumerable.Concat(
            image.GetEdgesClockwise(),
            image.GetEdgesAntiClockwise());

    public static IEnumerable<Edge> GetEdgesClockwise(this IImage image)
    {
        yield return image.GetEdgeUa();
        yield return image.GetEdgeRa();
        yield return image.GetEdgeDa();
        yield return image.GetEdgeLa();
    }
    public static IEnumerable<Edge> GetEdgesAntiClockwise(this IImage image)
    {
        yield return image.GetEdgeUb();
        yield return image.GetEdgeLb();
        yield return image.GetEdgeDb();
        yield return image.GetEdgeRb();
    }

    public static Edge GetEdgeUa(this IImage image) => new Edge(image.Scan(MoreEnumerable.Sequence(0, image.MaxX()), y:0));
    public static Edge GetEdgeRa(this IImage image) => new Edge(image.Scan(image.MaxX(), MoreEnumerable.Sequence(0, image.MaxY())));
    public static Edge GetEdgeDa(this IImage image) => new Edge(image.Scan(MoreEnumerable.Sequence(image.MaxX(), 0), image.MaxY()));
    public static Edge GetEdgeLa(this IImage image) => new Edge(image.Scan(x:0, MoreEnumerable.Sequence(image.MaxX(), 0)));

    public static Edge GetEdgeUb(this IImage image) => image.GetEdgeUa().Reverse();
    public static Edge GetEdgeRb(this IImage image) => image.GetEdgeRa().Reverse();
    public static Edge GetEdgeDb(this IImage image) => image.GetEdgeDa().Reverse();
    public static Edge GetEdgeLb(this IImage image) => image.GetEdgeLa().Reverse();

    private static IEnumerable<bool> Scan(this IImage image, int x, IEnumerable<int> ys) => ys.Select(y => image.Get(x, y));
    private static IEnumerable<bool> Scan(this IImage image, IEnumerable<int> xs, int y) => xs.Select(x => image.Get(x, y));
    private static bool Get(this IImage image, int x, int y) => image[(x, y)];
}
