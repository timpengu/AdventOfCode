using AoC_2025_12.GameModel.Coords;

namespace AoC_2025_12.GameModel.Game;

public sealed record OrientedPiece : IHasPositions
{
    public Piece Piece { get; }
    public Orientation Orientation { get; }
    public IReadOnlyList<Coord> Positions { get; }
    public CoordRange Bounds { get; }

    internal OrientedPiece(Piece piece, Orientation orientation, IEnumerable<Coord> positions)
    {
        Piece = piece;
        Orientation = orientation;
        Positions = positions.Normalise().ToList();
        Bounds = Positions.GetBounds();
    }

    IEnumerable<Coord> IHasPositions.Positions => Positions;

    public override string ToString() => $"{Piece.Name}:{Orientation}";
}
