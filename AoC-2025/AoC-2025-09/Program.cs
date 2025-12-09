using MoreLinq;
using System.Diagnostics;

List<Coord> coords = File.ReadLines("input.txt")
    .Select(s => s.Split(",", 2, StringSplitOptions.TrimEntries).Select(int.Parse).ToArray())
    .Select(a => new Coord(a[0], a[1]))
    .ToList();

var perimeter = GetPerimeter(coords).MergeColinear().ToList();
Console.WriteLine($"\nPerimeter: {String.Join(",", perimeter)}\n");

var coordPairsDescByArea =
    from z1 in coords
    from z2 in coords
    let area = Area(z1, z2)
    orderby area descending
    select (Z1:z1, Z2:z2, Area:area);

var big1 = coordPairsDescByArea.First();
Console.WriteLine($"Part 1: {big1.Z1} {big1.Z2} => {big1.Area}\n");

var big2 = coordPairsDescByArea.First(s =>
    GetRectangleEdges(s.Z1, s.Z2).All(r =>
        perimeter.All(p =>
            !p.IsIntersectedBy(r.Edge, r.OutwardNormal)))
);
Console.WriteLine($"Part 2: {big2.Z1} {big2.Z2} => {big2.Area}\n");

static long Area(Coord z1, Coord z2) => ((long)Math.Abs(z1.X - z2.X) + 1) * ((long)Math.Abs(z1.Y - z2.Y) + 1);

static IEnumerable<Edge> GetPerimeter(IReadOnlyCollection<Coord> coords) =>
    coords
        .Append(coords.First())
        .Pairwise((a, b) => new Edge(a, b));

static IEnumerable<(Edge Edge, Coord OutwardNormal)> GetRectangleEdges(Coord a, Coord b)
{
    int[] xs = a.X < b.X ? [a.X, b.X] : [b.X, a.X];
    int[] ys = a.Y < b.Y ? [a.Y, b.Y] : [b.Y, a.Y];

    Coord z00 = (xs[0], ys[0]);
    Coord z01 = (xs[0], ys[1]);
    Coord z11 = (xs[1], ys[1]);
    Coord z10 = (xs[1], ys[0]);

    return [
        (new (z00, z01), (-1,0)),
        (new (z01, z11), (0,+1)),
        (new (z11, z10), (+1,0)),
        (new (z10, z00), (0,-1))
    ];
}

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    
    public override string ToString() => $"({X},{Y})";
}

record struct Edge(Coord Start, Coord End)
{
    public static Edge operator +(Edge l, Coord z) => new(l.Start + z, l.End + z);

    public bool IsVertical => Start.X == End.X;
    public bool IsHorizontal => Start.Y == End.Y;
    
    public int MinX => Math.Min(Start.X, End.X);
    public int MinY => Math.Min(Start.Y, End.Y);
    public int MaxX => Math.Max(Start.X, End.X);
    public int MaxY => Math.Max(Start.Y, End.Y);
    
    public override string ToString() => $"{Start}-{End}";
}

internal static class Extensions
{
    public static bool IsIntersectedBy(this Edge perim, Edge edge, Coord edgeOutwardNormal)
    {
        perim += edgeOutwardNormal; // HACK: secret sauce :)

        if (edge.MaxX < perim.MinX || edge.MinX > perim.MaxX || edge.MaxY < perim.MinY || edge.MinY > perim.MaxY)
        {
            return false;
        }
        if (edge.IsHorizontal && perim.IsVertical)
        {
            return edge.MinX < perim.Start.X && edge.MaxX > perim.Start.X;
        }
        if (edge.IsVertical && perim.IsHorizontal)
        {
            return edge.MinY < perim.Start.Y && edge.MaxY > perim.Start.Y;
        }

        return false;
    }

    // Merge consecutive colinear edges to avoid nasty edge cases (not actually needed for the given input)
    public static IEnumerable<Edge> MergeColinear(this IEnumerable<Edge> edges)
    {
        var e = edges.GetEnumerator();
        if (!e.MoveNext()) yield break;

        Edge edge = e.Current;
        while (e.MoveNext())
        {
            var edgeNext = e.Current;
            if (edge.IsHorizontal && edgeNext.IsHorizontal)
            {
                Debug.Assert(edge.Start.Y == edgeNext.Start.Y);
                var y = edge.Start.Y;
                var minX = Math.Min(edge.MinX, edgeNext.MinX);
                var maxX = Math.Max(edge.MaxX, edgeNext.MaxX);
                edge = new((minX, y), (maxX, y));
            }
            else if (edge.IsVertical && edgeNext.IsVertical)
            {
                Debug.Assert(edge.Start.X == edgeNext.Start.X);
                var x = edge.Start.X;
                var minY = Math.Min(edge.MinY, edgeNext.MinY);
                var maxY = Math.Max(edge.MaxY, edgeNext.MaxY);
                edge = new((x, minY), (x, maxY));
            }
            else
            {
                yield return edge;
                edge = e.Current;
            }
        }
        yield return edge;
    }
}
