using MoreLinq;
using System.Diagnostics;

internal class KeypadEncoder
{
    private IDictionary<(char FromKey, Coord dZ), char> _toInnerKey;
    private ILookup<(char FromKey, char ToKey), string> _toOuterSequence;

    private Dictionary<(string Sequence, int Expansions), long> _memoizedExpansionLength = new();

    public KeypadEncoder(IEnumerable<KeyPosition> keyPositions)
    {
        var keyPositionsList = keyPositions.ToList();
        _toInnerKey = BuildInnerKeyDictionary(keyPositionsList);
        _toOuterSequence = BuildOuterSequenceLookup(keyPositionsList);
    }

    public string DecodeInnerSequence(string outerSequence, char startKey = 'A')
    {
        Debug.Assert(outerSequence[^1] == 'A'); // sequences should always end with A
        return string.Concat(outerSequence
            .SplitSequence() // split into subsequences ending in A
            .Select(seq => seq.ToOffset())
            .Scan(startKey, (fromKey, offset) => _toInnerKey[(fromKey, offset)]) // reconstruct the sequence of inner keys
            .Skip(1) // skip the start key
        );
    }

    public IList<string> EncodeOuterSequences(IEnumerable<string> innerSequences, char startKey = 'A')
    {
        return innerSequences
            .SelectMany(seq => EncodeOuterSequences(seq, startKey))
            .WhereMinBy(seq => seq.Length)
            .ToList();
    }

    public long GetEncodedOuterSequenceMinLength(string innerSequence, int encodingLevels = 1)
    {
        return encodingLevels == 0
            ? innerSequence.Length
            : innerSequence
                .SplitSequence() // split into subsequences ending with A
                .Sum(GetExpansionLength); // expand each subsequence separately for efficient memoization

        long GetExpansionLength(string sequence)
        {
            if (!_memoizedExpansionLength.TryGetValue((sequence, encodingLevels), out long expandedLength))
            {
                expandedLength = EncodeOuterSequences(sequence)
                    .Min(outerSequence =>
                        GetEncodedOuterSequenceMinLength(outerSequence, encodingLevels - 1));

                Console.WriteLine($"+{encodingLevels}: {sequence} => min length {expandedLength}");

                _memoizedExpansionLength.Add((sequence, encodingLevels), expandedLength);
            }

            return expandedLength;
        }
    }

    private IEnumerable<string> EncodeOuterSequences(string innerSequence, char startKey = 'A')
    {
        Debug.Assert(innerSequence[^1] == 'A'); // sequences should always end with A
        return Encode(innerSequence, startKey).ToList();

        IEnumerable<string> Encode(ReadOnlySpan<char> innerSequence, char fromKey)
        {
            if (innerSequence.Length == 0)
            {
                return [String.Empty];
            }

            char toKey = innerSequence[0];
            IEnumerable<string> outerSequencePrefixes = _toOuterSequence[(fromKey, toKey)];
            IEnumerable<string> outerSequenceSuffixes = Encode(innerSequence[1..], toKey);
            return outerSequencePrefixes.Cartesian(outerSequenceSuffixes, (s1, s2) => s1 + s2);
        }
    }

    private static IDictionary<(char FromKey, Coord dZ), char> BuildInnerKeyDictionary(IReadOnlyCollection<KeyPosition> keyPositions) =>
        keyPositions
            .ToKeyOffsets()
            .ToDictionary(
                v => (v.FromKey, v.dZ),
                v => v.ToKey);

    private static ILookup<(char FromKey, char ToKey), string> BuildOuterSequenceLookup(IReadOnlyCollection<KeyPosition> keyPositions)
    {
        IDictionary<char, Coord> zKeys = keyPositions.ToDictionary(k => k.Key, k => k.Z);
        ISet<Coord> zAllKeys = keyPositions.Select(k => k.Z).ToHashSet();

        bool IsValidSequence(string outerKeySequence, char fromKey) =>
            outerKeySequence
                .Scan(zKeys[fromKey], (z, key) => z + key.ToOffset()) // get each interim position in the sequence
                .All(zAllKeys.Contains); // check each position is over a key (not a gap)

        return keyPositions
            .ToKeyOffsets()
            .SelectMany(
                offset => offset.dZ.GetOuterSequences(),
                (offset, seq) => (offset.FromKey, offset.ToKey, OuterKeySequence: seq))
            .Where(o => IsValidSequence(o.OuterKeySequence, o.FromKey))
            .ToLookup(o => (o.FromKey, o.ToKey), o => o.OuterKeySequence);
    }
}
