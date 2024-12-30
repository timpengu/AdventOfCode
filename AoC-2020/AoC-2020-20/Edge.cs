using MoreLinq;
using MoreLinq.Extensions;

public record struct Edge: IEquatable<Edge>
{
    public readonly bool[] Pattern;

    public Edge(IEnumerable<bool> pattern)
    {
        Pattern = pattern.ToArray();
    }

    public Edge Reverse() => new Edge(Pattern.Reverse());

    public bool Equals(Edge other) =>
        Pattern.SequenceEqual(other.Pattern);

    public override int GetHashCode() =>
        Pattern.Aggregate(1, (hash, bit) =>
            unchecked((hash << 1) + (bit ? 1 : 0)) % int.MaxValue);

    public override string ToString() => String.Concat(Pattern.Select(c => c ? '#' : '.'));
}
