namespace AoC_2025_12.GameModel.Game;

/// <summary>
/// Compares pieces according to a heuristic placement difficulty metric
/// </summary>
public sealed class PlacementDifficultyComparer : IComparer<Piece>
{
    public int Compare(Piece? a, Piece? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (ReferenceEquals(a, null)) return -1;
        if (ReferenceEquals(b, null)) return +1;

        int comparePositions = a.PositionCount.CompareTo(b.PositionCount); // more positions are more difficult to place
        if (comparePositions != 0) return comparePositions;

        // TODO: Consider eccentricity? i.e. longest dimension divided by shortest dimension

        int compareOrientations = b.Orientations.Count.CompareTo(a.Orientations.Count); // fewer orientations are more difficult to place
        if (compareOrientations != 0) return compareOrientations;

        return 0; // otherwise similar difficulty
    }
}
