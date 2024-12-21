
using MoreLinq;
using System.Diagnostics;

internal static class Program
{
    private static void Main(string[] args)
    {
        IList<string> doorCodes = File.ReadLines("inputSample.txt").ToList();

        List<KeyOffset> npadOffsets = GetKeyOffsets("789", "456", "123", " 0A").ToList();
        List<KeyOffset> dpadOffsets = GetKeyOffsets(" ^A", "<v>").ToList();

        IDictionary<(char FromKey, Coord dZ), char> toNpad = npadOffsets.ToInnerKeyDictionary();
        IDictionary<(char FromKey, Coord dZ), char> toDpad = dpadOffsets.ToInnerKeyDictionary();

        IDictionary<(char FromKey, char ToKey), string> fromNpad = npadOffsets.ToOuterKeySequenceDictionary();
        IDictionary<(char FromKey, char ToKey), string> fromDpad = dpadOffsets.ToOuterKeySequenceDictionary();

        int complexityTotal = 0;
        
        foreach (string doorCode in doorCodes)
        {
            string robo1Code = fromNpad.GetOuterCode(doorCode);
            string robo2Code = fromDpad.GetOuterCode(robo1Code);
            string humanCode = fromDpad.GetOuterCode(robo2Code);

            // Invert codes to check no fuckups
            string robo2CodeR = toDpad.GetInnerCode(humanCode);
            string robo1CodeR = toDpad.GetInnerCode(robo2CodeR);
            string doorCodeR = toNpad.GetInnerCode(robo1CodeR);
            Debug.Assert(robo2CodeR == robo2Code);
            Debug.Assert(robo1CodeR == robo1Code);
            Debug.Assert(doorCodeR == doorCode);

            int numericCode = int.Parse(doorCode.Trim('A'));
            int complexity = humanCode.Length * numericCode;

            complexityTotal += complexity;

            Console.WriteLine(doorCode);
            Console.WriteLine(robo1Code);
            Console.WriteLine(robo2Code);
            Console.WriteLine(humanCode);
            Console.WriteLine($"[{humanCode.Length} * {numericCode} = {complexity}]\n");
        }

        Console.WriteLine($"Total complexity: {complexityTotal}");
    }

    private static string GetInnerCode(
        this IDictionary<(char FromKey, Coord dZ), char> nextChars, string code, char startKey = 'A')
    {
        Debug.Assert(code[^1] == 'A'); // key codes should always end with A
        return string.Concat(code
            .Split('A') // split out each move sequence (before pressing A to select the key)
            .SkipLast(1) // discard empty sequence after final A
            .Select(GetOffset)
            .Scan(startKey, (fromKey, offset) => nextChars[(fromKey, offset)]) // reconstruct the sequence of keys
            .Skip(1) // skip the initial A key
        );
    }

    private static string GetOuterCode(
        this IDictionary<(char FromKey, char ToKey), string> seqs, string code, char startKey = 'A') =>
        string.Concat(code
            .Prepend(startKey)
            .Pairwise((c1, c2) => (FromKey: c1, ToKey: c2)) // get each move between keys
            .Select(keyPair => seqs[keyPair]) // get the sequence for each move
        );

    private static IDictionary<(char FromKey, Coord dZ), char> ToInnerKeyDictionary(this IEnumerable<KeyOffset> keyOffsets) =>
        keyOffsets.ToDictionary(
            v => (v.FromKey, v.dZ),
            v => v.ToKey);

    private static IDictionary<(char FromKey, char ToKey), string> ToOuterKeySequenceDictionary(this IEnumerable<KeyOffset> keyOffsets) =>
        keyOffsets.ToDictionary(
            v => (v.FromKey, v.ToKey),
            v => v.dZ.GetOuterKeySequence());

    private static string GetOuterKeySequence(this Coord dz)
    {
        string seq = string.Empty;
        if (dz.X > 0)
        {
            // Move right first to avoid gaps in keypads
            seq += GetKeySequenceX(dz.X);
        }

        seq += GetKeySequenceY(dz.Y);

        if (dz.X < 0)
        {
            // Move left last to avoid gaps in keypads
            seq += GetKeySequenceX(dz.X);
        }

        return seq + 'A'; // press A to select the button
    }

    private static string GetKeySequenceX(int dx) => dx switch
    {
        > 0 => new string('>', dx),
        < 0 => new string('<', -dx),
        _ => string.Empty
    };

    private static string GetKeySequenceY(int dy) => dy switch
    {
        > 0 => new string('v', dy),
        < 0 => new string('^', -dy),
        _ => string.Empty
    };

    private static Coord GetOffset(this string keySequence) => keySequence.Aggregate(new Coord(0, 0), (dz, key) => dz + GetOffset(key));
    private static Coord GetOffset(this char key) => key switch
    {
        '>' => (+1, 0),
        'v' => (0, +1),
        '<' => (-1, 0),
        '^' => (0, -1),
        _ => throw new ArgumentException($"Invalid key: '{key}'")
    };

    private static IEnumerable<KeyOffset> GetKeyOffsets(params string[] keyLines) => GetKeyPositions(keyLines).GetKeyOffsets();
    private static IEnumerable<KeyOffset> GetKeyOffsets(this IEnumerable<KeyPosition> keyPositions)
    {
        List<KeyPosition> keyPositionList = keyPositions.ToList();
        foreach (var fromKey in keyPositionList)
        {
            foreach (var toKey in keyPositionList)
            {
                yield return new KeyOffset(fromKey.Key, toKey.Key, toKey.Z - fromKey.Z);
            }
        }
    }

    private static IEnumerable<KeyPosition> GetKeyPositions(params string[] keyLines)
    {
        Debug.Assert(keyLines.Select(l => l.Length).Distinct().Count() == 1);

        for (int j = 0; j < keyLines.Length; ++j)
        {
            for (int i = 0; i < keyLines[j].Length; ++i)
            {
                char c = keyLines[j][i];
                if (!char.IsWhiteSpace(c))
                {
                    yield return new KeyPosition(c, (i, j));
                }
            }
        }
    }
}

record struct KeyPosition(char Key, Coord Z);
record struct KeyOffset(char FromKey, char ToKey, Coord dZ);
record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => (a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => (a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X},{Y})";
}
