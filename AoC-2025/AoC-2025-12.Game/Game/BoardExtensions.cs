using AoC_2025_12.GameModel.Coords;
using MoreLinq;

namespace AoC_2025_12.GameModel.Game;

public static class BoardExtensions
{
    public static IEnumerable<Coord> OccupiedPositions(this Board board) => board.Where(board.IsOccupied);
    public static IEnumerable<Coord> UnoccupiedPositions(this Board board) => board.Where(coord => !board.IsOccupied(coord));
    private static IEnumerable<Coord> Where(this Board board, Func<Coord, bool> predicate) => board.Bounds.EnumerateCoords().Where(predicate);

    /// <summary>
    /// Returns a board with the given number of additional random occupied positions (or fewer if the board is full)
    /// </summary>
    public static Board WithOccupiedRandomPositions(this Board board, int randomPositions) =>
        board.WithOccupiedPositions(
            board.UnoccupiedPositions()
                .Shuffle() // generate random permutation of unoccupied positions
                .Take(randomPositions)); // take the first N (if available)

    /// <summary>
    /// Gets the number of surplus positions that will remain once all the given pieces are placed on the board
    /// </summary>
    /// <returns>The number of surplus positions, which may be positive, zero, or negative if there is insufficient space for the pieces</returns>
    public static int CountSurplusPositions(this Board board, IEnumerable<Piece> pieces)
    {
        int unoccupiedPositions = board.UnoccupiedPositions().Count();
        int placementPositions = pieces.Sum(p => p.PositionCount);
        return unoccupiedPositions - placementPositions;
    }
}
