using AoC_2025_12.GameModel.Coords;

namespace AoC_2025_12.GameModel.Game;

public record struct Placement(OrientedPiece OrientedPiece, Coord Offset) : IHasPositions
{
    public IEnumerable<Coord> Positions => OrientedPiece.Positions.Transpose(Offset);

    public override string ToString() => $"{OrientedPiece}@{Offset} => [{string.Join(",", Positions)}]";
}
