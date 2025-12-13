using System.Diagnostics.CodeAnalysis;

namespace AoC_2025_12.GameModel.Game;

public sealed class PositionsComparer<T> : IEqualityComparer<T> where T : IHasPositions
{
    public bool Equals(T? a, T? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (ReferenceEquals(a, null)) return false;
        if (ReferenceEquals(b, null)) return false;

        return a.Positions.SequenceEqual(b.Positions);
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        unchecked
        {
            return obj.Positions.Aggregate(19, (hashCode, position) => 31 * hashCode + position.GetHashCode());
        }
    }
}
