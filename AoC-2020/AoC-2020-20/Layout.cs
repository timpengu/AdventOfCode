using MoreLinq;
using MoreLinq.Extensions;
using System.Diagnostics;

class Layout : IImage
{
    private readonly Coord _imageSize;
    private readonly OrientedTile[,] _tiles;

    public Layout(IReadOnlyList<IReadOnlyList<OrientedTile>> tiles)
    {
        // ensure all tiles have the same dimensions
        Debug.Assert(tiles.SelectMany(t => t).Select(t => t.Size).Distinct().Count() == 1);
        _imageSize = tiles[0][0].GetImage().Size;

        int xs = tiles.Select(row => row.Count).Distinct().Single();
        int ys = tiles.Count;
        _tiles = new OrientedTile[xs, ys];
        for (int y = 0; y < ys; ++y)
        {
            for (int x = 0; x < xs; ++x)
            {
                _tiles[x, y] = tiles[y][x];
            }
        }
    }

    public Coord TileCount => (_tiles.GetLength(0), _tiles.GetLength(1));
    public OrientedTile GetTile(Coord zTile) => _tiles[zTile.X, zTile.Y];

    public Coord Size => TileCount * _imageSize;
    public bool this[Coord z]
    {
        get
        {
            Image image = GetTile(z / _imageSize).GetImage();
            return image[z % _imageSize];
        }
    }
}
