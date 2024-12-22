using MoreLinq;
using System.Diagnostics;

internal static class Extensions
{
    public static IEnumerable<KeyOffset> ToKeyOffsets(this IReadOnlyCollection<KeyPosition> keyPositions) =>
        keyPositions.Cartesian(
            keyPositions,
            (from, to) => new KeyOffset(from.Key, to.Key, to.Z - from.Z));

    public static IEnumerable<string> GetOuterSequences(this Coord dz)
    {
        string seqX = GetOuterSequenceX(dz.X);
        string seqY = GetOuterSequenceY(dz.Y);

        yield return string.Concat(seqX, seqY, 'A');

        if (seqX.Length > 0 && seqY.Length > 0)
        {
            // also return the Y-X order sequence
            yield return string.Concat(seqY, seqX, 'A');
        }
    }

    private static string GetOuterSequenceX(int dx) => dx switch
    {
        > 0 => new string('>', dx),
        < 0 => new string('<', -dx),
        _ => string.Empty
    };

    private static string GetOuterSequenceY(int dy) => dy switch
    {
        > 0 => new string('v', dy),
        < 0 => new string('^', -dy),
        _ => string.Empty
    };

    public static Coord ToOffset(this string sequence) => sequence.Aggregate(new Coord(0, 0), (dz, key) => dz + ToOffset(key));
    public static Coord ToOffset(this char key) => key switch
    {
        '>' => (+1, 0),
        'v' => (0, +1),
        '<' => (-1, 0),
        '^' => (0, -1),
        'A' => (0, 0),
        _ => throw new ArgumentException($"Invalid key: '{key}'")
    };

    public static IEnumerable<string> SplitSequence(this string sequence)
    {
        Debug.Assert(sequence[^1] == 'A'); // valid sequences should always end in A
        int startIndex = 0;
        int nextIndex = sequence.IndexOf('A');
        while (nextIndex >= 0)
        {
            yield return sequence[startIndex..(nextIndex + 1)];
            
            startIndex = nextIndex + 1;
            nextIndex = sequence.IndexOf('A', startIndex);
        }
    }

    public static IEnumerable<TSource> WhereMinBy<TSource, TValue>(
        this IEnumerable<TSource> source, Func<TSource, TValue> selector)
        where TValue : IEquatable<TValue>
    {
        List<TSource> items = source.ToList();
        if (items.Count == 0)
        {
            return items;
        }
        TValue? minValue = items.Min(selector);
        return items.Where(node => selector(node).Equals(minValue));
    }
}
