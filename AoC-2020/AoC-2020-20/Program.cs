using MoreLinq;
using MoreLinq.Extensions;
using System.Diagnostics;
using System.Text.RegularExpressions;

List<Tile> tiles = new();

using (StreamReader file = new("inputSample.txt"))
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
Debug.Assert(tiles.Select(t => t.Size).Distinct().Count() == 1);

ILookup<Edge, Tile> tilesByEdge = tiles
    .SelectMany(
        tile => tile.GetEdges(),
        (tile, edge) => (Edge:edge, Tile:tile))
    .ToLookup(t => t.Edge, t => t.Tile);

HashSet<Tile> cornerTiles = new();
foreach(Tile tile in tiles.OrderBy(t => t.Id))
{
    Console.WriteLine($"\nTile {tile.Id}:");

    var edges = tile
        .GetEdgesClockwise()
        .Zip(['U', 'R', 'D', 'L']) // edge facing directions
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
        cornerTiles.Add(tile);
    }
}

long cornerIdProduct = cornerTiles.Aggregate(1L, (product, tile) => product * tile.Id);
Console.WriteLine($"\nFound {cornerTiles.Count} corners with Id product: {cornerIdProduct}");

List<Layout> layouts = new LayoutEngine(tiles).GenerateLayouts().ToList();
for (int i = 0; i < layouts.Count; ++i)
{
    var layout = layouts[i];

    Console.WriteLine($"\nLayout {i + 1}:");
    for (int y = 0; y < layout.TileCount.Y; ++y)
    {
        for (int x = 0; x < layout.TileCount.X; ++x)
        {
            var tile = layout.GetTile((x, y));
            Console.Write($"{tile,8}");
        }
        Console.WriteLine();
    }
    for (int y = 0; y < layout.Size.Y; ++y)
    {
        for (int x = 0; x < layout.Size.X; ++x)
        {
            bool value = layout[(x, y)];
            Console.Write(value ? '#' : '.');
        }
        Console.WriteLine();
    }
}
