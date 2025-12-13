using AoC_2025_12.GameModel.Coords;

namespace AoC_2025_12.GameModel.Game;

public static class PlacementExtensions
{
    /// <summary>
    /// Constructs a dictionary of Piece by Coord for a sequence of non-overlapping Placements
    /// </summary>
    public static IDictionary<Coord, Piece> ToLayout(this IEnumerable<Placement> placements) =>
        new Dictionary<Coord, Piece>(
            placements.SelectMany(
                placement => placement.Positions,
                (placement, position) => KeyValuePair.Create(position, placement.OrientedPiece.Piece))
        );
}
