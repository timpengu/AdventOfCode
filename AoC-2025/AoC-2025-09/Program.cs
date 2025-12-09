using MoreLinq;
using System.Diagnostics;

List<Coord> coords = File.ReadLines("input.txt")
    .Select(s => s.Split(",", 2, StringSplitOptions.TrimEntries).Select(int.Parse).ToArray())
    .Select(a => new Coord(a[0], a[1]))
    .ToList();

var perimeter = GetPerimeter(coords).MergeColinear().ToList();
Console.WriteLine($"\nPerimeter: {String.Join(",", perimeter)}\n");

var rectsByAreaDesc =
    from z1 in coords
    from z2 in coords
    let rect = Rect.Create(z1, z2)
    let area = Area(z1, z2)
    orderby area descending
    select (Rect:rect, Area:area);

var big1 = rectsByAreaDesc.First();
Console.WriteLine($"Part 1: {big1.Rect} => {big1.Area}\n");

var big2 = rectsByAreaDesc.First(s =>
    perimeter.All(p => !p.Intersects(s.Rect))
);
Console.WriteLine($"Part 2: {big2.Rect} => {big2.Area}\n");

static long Area(Coord z1, Coord z2) => ((long)Math.Abs(z1.X - z2.X) + 1) * ((long)Math.Abs(z1.Y - z2.Y) + 1);

static IEnumerable<Edge> GetPerimeter(IReadOnlyCollection<Coord> coords) =>
    coords
        .Append(coords.First())
        .Pairwise((a, b) => new Edge(a, b));

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    
    public override string ToString() => $"({X},{Y})";
}

record struct Edge(Coord Start, Coord End)
{
    public bool IsVertical => Start.X == End.X;
    public bool IsHorizontal => Start.Y == End.Y;
    
    public int MinX => Math.Min(Start.X, End.X);
    public int MinY => Math.Min(Start.Y, End.Y);
    public int MaxX => Math.Max(Start.X, End.X);
    public int MaxY => Math.Max(Start.Y, End.Y);
    
    public override string ToString() => $"{Start}-{End}";
}

record struct Rect(Coord Min, Coord Max)
{
    public static Rect Create(Coord a, Coord b) => new Rect
    (
        (Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)),
        (Math.Max(a.X, b.X), Math.Max(a.Y, b.Y))
    );

    public override string ToString() => $"{Min}-{Max}";
}

internal static class Extensions
{
    public static bool Intersects(this Edge perim, Rect r)
    {
        if (r.Max.X < perim.MinX || r.Min.X > perim.MaxX || r.Max.Y < perim.MinY || r.Min.Y > perim.MaxY)
        {
            return false;
        }
        if (perim.IsHorizontal)
        {
            return r.Min.Y < perim.Start.Y && r.Max.Y > perim.Start.Y;
        }
        if (perim.IsVertical)
        {
            return r.Min.X < perim.Start.X && r.Max.X > perim.Start.X;
        }
        throw new ArgumentException(nameof(perim));
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
