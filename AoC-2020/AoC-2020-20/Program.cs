using MoreLinq;
using MoreLinq.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<Tile> tiles = new();

using (StreamReader file = new("input.txt"))
{
    while (!file.EndOfStream)
    {
        // skip blank lines and read tile header
        string? line = file.ReadLine();
        while (line?.Length == 0)
        {
            line = file.ReadLine();
        }
        Match match = Regex.Match(line ?? String.Empty, @"^Tile ([0-9]+):$");
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out int id))
        {
            throw new Exception($"Cannot parse tile header: '{line}'");
        }

        // read tile image
        List<string> lines = new();
        for (line = file.ReadLine(); line?.Length > 0; line = file.ReadLine())
        {
            lines.Add(line.Trim());
        }
        int ys = lines.Count;
        int xs = lines.Select(p => p.Length).Distinct().Single();
        bool[,] image = new bool[xs, ys];
        for (int y = 0; y < ys; ++y)
        {
            for (int x = 0; x < xs; ++x)
            {
                char c = lines[y][x];
                image[x, y] = c switch
                {
                    '#' => true,
                    '.' => false,
                    _ => throw new Exception($"Invalid image char '{c}'")
                };
            }
        }

        tiles.Add(new Tile(id, image));
    }
}

// ensure all tiles are the same size
Debug.Assert(tiles.Select(t => (t.XSize, t.YSize)).Distinct().Count() == 1);

ILookup<Edge, Tile> tilesByEdge = tiles
    .SelectMany(
        tile => tile.GetEdges(),
        (tile, edge) => (Edge:edge, Tile:tile))
    .ToLookup(t => t.Edge, t => t.Tile);

List<int> cornerIds = new();
foreach(var tile in tiles.OrderBy(t => t.Id))
{
    Console.WriteLine($"\nTile {tile.Id}:");

    var edges = tile
        .GetEdgesClockwise()
        .Zip(['N', 'E', 'S', 'W']) // edge facing directions
        .Select(edge => (
            Edge: edge.First,
            Facing: edge.Second,
            Neighbours: tilesByEdge[edge.First].Where(t => t != tile).ToList()
        ))
        .ToList();

    foreach (var edge in edges)
    {
        Console.WriteLine($"{edge.Facing}: {edge.Edge} {String.Join(",", edge.Neighbours.Select(n => n.Id))}");
    }

    int neighbours = edges.Count(e => e.Neighbours.Any());
    if (neighbours == 2)
    {
        Console.WriteLine("Corner!");
        cornerIds.Add(tile.Id);
    }
}

long cornerIdProduct = cornerIds.Aggregate(1L, (product, id) => product * id);
Console.WriteLine($"\nFound {cornerIds.Count} corners with Id product: {cornerIdProduct}");


record struct Edge: IEquatable<Edge>
{
    public readonly bool[] Pattern;

    public Edge(IEnumerable<bool> pattern)
    {
        Pattern = pattern.ToArray();
    }

    public Edge Flip() => new Edge(Pattern.Reverse());

    public bool Equals(Edge other) =>
        Pattern.SequenceEqual(other.Pattern);

    public override int GetHashCode() =>
        Pattern.Aggregate(1, (hash, bit) =>
            unchecked((hash << 1) + (bit ? 1 : 0)) % int.MaxValue);

    public override string ToString() => String.Concat(Pattern.Select(c => c ? '#' : '.'));
}

record Tile(int Id, bool[,] Image)
{
    public IEnumerable<Edge> GetEdges() => GetEdgesClockwise().Concat(GetEdgesAntiClockwise());
    public IEnumerable<Edge> GetEdgesAntiClockwise() => GetEdgesClockwise().Reverse().Select(e => e.Flip());
    public IEnumerable<Edge> GetEdgesClockwise()
    {
        yield return new Edge(Scan(MoreEnumerable.Sequence(0, XMax), 0));
        yield return new Edge(Scan(XMax, MoreEnumerable.Sequence(0, YMax)));
        yield return new Edge(Scan(MoreEnumerable.Sequence(XMax, 0), YMax));
        yield return new Edge(Scan(0, MoreEnumerable.Sequence(YMax, 0)));
    }

    private IEnumerable<bool> Scan(int x, IEnumerable<int> ys) => ys.Select(y => Image[x, y]);
    private IEnumerable<bool> Scan(IEnumerable<int> xs, int y) => xs.Select(x => Image[x, y]);

    public int XMax => XSize - 1;
    public int YMax => YSize - 1;
    public int XSize => Image.GetLength(0);
    public int YSize => Image.GetLength(1);
}
