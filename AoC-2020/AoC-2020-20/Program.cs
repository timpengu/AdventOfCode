using MoreLinq;
using System.Diagnostics;

List<Tile> tiles = new(File.ReadLines("input.txt").ParseTiles());
Pattern pattern = new(File.ReadLines("pattern.txt").ParsePatternCoords());

Debug.Assert(tiles.Select(t => t.Size).Distinct().Count() == 1); // ensure all tiles are the same size

LayoutEngine layoutEngine = new(tiles);

// part 1
List<Tile> cornerTiles = layoutEngine.GetCornerTiles().OrderBy(t => t.Id).ToList();
long cornerIdProduct = cornerTiles.Aggregate(1L, (product, tile) => product * tile.Id);
Console.WriteLine($"Corner tiles: {String.Join(' ', cornerTiles.Select(t => t.Id))}");
Console.WriteLine($"Found {cornerTiles.Count} corners with Id product: {cornerIdProduct}");
Debug.Assert(cornerTiles.Count == 4); // should have 4 corner tiles

// part 2
foreach (Layout layout in layoutEngine.GenerateLayouts())
{
    List<Coord> zMatchingPositions = pattern.FindMatchingPositions(layout).ToList();
    if (!zMatchingPositions.Any())
    {
        continue; // ignore layout orientations with no matching patterns
    }

    Console.WriteLine("\nLayout:");    
    foreach (int y in layout.TileRangeY())
    {
        foreach (int x in layout.TileRangeX())
        {
            OrientedTile tile = layout.GetTile((x, y));
            Console.Write($"{tile,8}");
        }
        Console.WriteLine();
    }
    Console.WriteLine();

    ISet<Coord> zPatternSet = pattern.GetMatchingCoords(zMatchingPositions).ToHashSet();
    foreach (int y in layout.RangeY())
    {
        foreach (int x in layout.RangeX())
        {
            Coord z = (x, y);
            bool isSet = layout[z];
            bool isPattern = isSet && zPatternSet.Contains(z);

            Console.ForegroundColor = isPattern ? ConsoleColor.Green : ConsoleColor.White;
            Console.Write(isSet ? isPattern ? 'O' : '#' : '.');
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    Console.WriteLine($"\nMatched {zMatchingPositions.Count} patterns at: {String.Join(' ', zMatchingPositions)}");

    int roughness = layout.Range().Count(z => layout[z] && !zPatternSet.Contains(z));
    Console.WriteLine($"\nRoughness: {roughness}");
}
