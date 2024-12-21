using MoreLinq;
using System.Diagnostics;

internal class KeypadEncoder
{
    private IDictionary<(char FromKey, Coord dZ), char> _toInnerKey;
    private ILookup<(char FromKey, char ToKey), string> _toOuterSequence;

    public KeypadEncoder(IEnumerable<KeyPosition> keyPositions)
    {
        var keyPositionsList = keyPositions.ToList();
        _toInnerKey = BuildInnerKeyDictionary(keyPositionsList);
        _toOuterSequence = BuildOuterSequenceLookup(keyPositionsList);
    }

    public string DecodeInnerSequence(string outerSequence, char startKey = 'A')
    {
        Debug.Assert(outerSequence[^1] == 'A'); // key codes should always end with A
        return string.Concat(outerSequence
            .Split('A') // split out each key sequence (before pressing A to select the key)
            .SkipLast(1) // discard empty sequence following final A
            .Select(k => k.GetOffset())
            .Scan(startKey, (fromKey, offset) => _toInnerKey[(fromKey, offset)]) // reconstruct the sequence of inner keys
            .Skip(1) // skip the initial A key
        );
    }

    public IList<string> EncodeOuterSequences(IEnumerable<string> innerSequences, char startKey = 'A')
    {
        return innerSequences
            .SelectMany(seq => EncodeOuterSequences(seq, startKey))
            .WhereMinBy(seq => seq.Length) // TOOD: apply a tolerance for minimum-ish length?
            .ToList();
    }

    public IEnumerable<string> EncodeOuterSequences(string innerSequence, char startKey = 'A')
    {
        Debug.Assert(innerSequence[^1] == 'A'); // key codes should always end with A
        List<string> results = Encode(innerSequence, startKey).ToList();
        return results;

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
                .Scan(zKeys[fromKey], (z, key) => z + key.GetOffset()) // get each interim position in the sequence
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
