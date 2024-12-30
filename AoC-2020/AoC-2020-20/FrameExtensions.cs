using MoreLinq;

public static class FrameExtensions
{
    public static Coord Max(this IImage frame) => (frame.MaxX(), frame.MaxY());
    public static int MaxX(this IImage frame) => frame.Size.X - 1;
    public static int MaxY(this IImage frame) => frame.Size.Y - 1;

    public static IEnumerable<Edge> GetEdges(this IImage frame) =>
        Enumerable.Concat(
            frame.GetEdgesClockwise(),
            frame.GetEdgesAntiClockwise());

    public static IEnumerable<Edge> GetEdgesClockwise(this IImage frame)
    {
        yield return frame.GetEdgeUa();
        yield return frame.GetEdgeRa();
        yield return frame.GetEdgeDa();
        yield return frame.GetEdgeLa();
    }
    public static IEnumerable<Edge> GetEdgesAntiClockwise(this IImage frame)
    {
        yield return frame.GetEdgeUb();
        yield return frame.GetEdgeLb();
        yield return frame.GetEdgeDb();
        yield return frame.GetEdgeRb();
    }

    public static Edge GetEdgeUa(this IImage frame) => new Edge(frame.Scan(MoreEnumerable.Sequence(0, frame.MaxX()), y:0));
    public static Edge GetEdgeRa(this IImage frame) => new Edge(frame.Scan(frame.MaxX(), MoreEnumerable.Sequence(0, frame.MaxY())));
    public static Edge GetEdgeDa(this IImage frame) => new Edge(frame.Scan(MoreEnumerable.Sequence(frame.MaxX(), 0), frame.MaxY()));
    public static Edge GetEdgeLa(this IImage frame) => new Edge(frame.Scan(x:0, MoreEnumerable.Sequence(frame.MaxX(), 0)));

    public static Edge GetEdgeUb(this IImage frame) => frame.GetEdgeUa().Reverse();
    public static Edge GetEdgeRb(this IImage frame) => frame.GetEdgeRa().Reverse();
    public static Edge GetEdgeDb(this IImage frame) => frame.GetEdgeDa().Reverse();
    public static Edge GetEdgeLb(this IImage frame) => frame.GetEdgeLa().Reverse();

    private static IEnumerable<bool> Scan(this IImage frame, int x, IEnumerable<int> ys) => ys.Select(y => frame.Get(x, y));
    private static IEnumerable<bool> Scan(this IImage frame, IEnumerable<int> xs, int y) => xs.Select(x => frame.Get(x, y));
    private static bool Get(this IImage frame, int x, int y) => frame[(x, y)];
}
