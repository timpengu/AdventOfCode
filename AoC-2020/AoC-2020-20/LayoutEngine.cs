using MoreLinq;
using MoreLinq.Extensions;
using System.Diagnostics;

class LayoutEngine
{
    private readonly IList<OrientedTile> _orientedTiles;
 
    private readonly ILookup<Edge, OrientedTile> _tilesRight;
    private readonly ILookup<Edge, OrientedTile> _tilesDown;
    private readonly ILookup<Edge, OrientedTile> _tilesLeft;
    private readonly ILookup<Edge, OrientedTile> _tilesUp;

    public LayoutEngine(IEnumerable<Tile> tiles)
    {
        _orientedTiles = tiles.SelectMany(OrientedTile.GetOrientations).ToList();

        // Create lookups for incoming edge connections (using reverse sense edge vectors)
        _tilesRight = GetConnectionsByEdge(tile => tile.GetEdgeLb());
        _tilesDown = GetConnectionsByEdge(tile => tile.GetEdgeUb());
        _tilesLeft = GetConnectionsByEdge(tile => tile.GetEdgeRb());
        _tilesUp = GetConnectionsByEdge(tile => tile.GetEdgeDb());

        ILookup<Edge, OrientedTile> GetConnectionsByEdge(Func<OrientedTile, Edge> edgeSelector)
        {
            var connections = _orientedTiles.ToLookup(edgeSelector);
            Debug.Assert(connections.All(g => g.Count() <= 2)); // each distinct edge should match at most two tiles
            return connections;
        }
    }

    // Lookup outgoing edge connections (using normal sense edge vectors)
    public OrientedTile? GetConnectedTileRight(OrientedTile tile) => GetConnectedTile(_tilesRight, tile, tile.GetEdgeRa());
    public OrientedTile? GetConnectedTileDown(OrientedTile tile) => GetConnectedTile(_tilesDown, tile, tile.GetEdgeDa());
    public OrientedTile? GetConnectedTileLeft(OrientedTile tile) => GetConnectedTile(_tilesLeft, tile, tile.GetEdgeLa());
    public OrientedTile? GetConnectedTileUp(OrientedTile tile) => GetConnectedTile(_tilesUp, tile, tile.GetEdgeUa());

    private static OrientedTile? GetConnectedTile(ILookup<Edge, OrientedTile> connectedTiles, OrientedTile tile, Edge edge) =>
        connectedTiles[edge].SingleOrDefault(connectedTile => connectedTile.Tile != tile.Tile);

    public IEnumerable<Layout> GenerateLayouts()
    {
        List<OrientedTile> topLeftTiles = _orientedTiles
            .Where(tile => GetConnectedTileRight(tile) != null)
            .Where(tile => GetConnectedTileDown(tile) != null)
            .Where(tile => GetConnectedTileLeft(tile) == null)
            .Where(tile => GetConnectedTileUp(tile) == null)
            .ToList();

        Debug.Assert(topLeftTiles.Count == 8); // should have 4 corner tiles x 2 orientations

        foreach (var topLeftTile in topLeftTiles)
        {
            List<List<OrientedTile>> tileLayout = new();
            List<OrientedTile>? prevRow = null;
            for (OrientedTile? tile = topLeftTile; tile != null;)
            {
                List<OrientedTile> tileRow = new();
                tileLayout.Add(tileRow);

                while (tile != null)
                {
                    if (prevRow != null)
                    {
                        OrientedTile upperTile = prevRow[tileRow.Count];
                        OrientedTile? connectedFromPrevRow = GetConnectedTileDown(upperTile);
                        Debug.Assert(tile == connectedFromPrevRow); // ensure tile top edge matches the previous row
                    }

                    tileRow.Add(tile);
                    tile = GetConnectedTileRight(tile);
                }

                prevRow = tileRow;
                tile = GetConnectedTileDown(tileRow[0]);
            }

            yield return new Layout(tileLayout);
        }
    }
}