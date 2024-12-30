record OrientedTile(Tile Tile, Orientation Orientation) : IImage
{
    public static IEnumerable<OrientedTile> GetOrientations(Tile tile) =>
        Orientation.GetAll().Select(orientation => new OrientedTile(tile, orientation));

    public Image GetImage() => new Image(this);

    public Coord Size => Coord.Abs(Orient(Tile.Size));
    public bool this[Coord z] => Tile[GetTileCoord(z)];

    private readonly Coord _tileOrigin = Tile.Max() * Orientation.Origin;
    private Coord GetTileCoord(Coord z) => _tileOrigin + Orient(z);
    private Coord Orient(Coord z) =>
        z.X * Orientation.dX +
        z.Y * Orientation.dY;

    public override string ToString() => $"{Tile.Id}:{Orientation.Name}";
}
