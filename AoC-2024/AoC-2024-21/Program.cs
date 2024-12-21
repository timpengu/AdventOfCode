using MoreLinq;
using System.Diagnostics;

internal static class Program
{
    private static void Main(string[] args)
    {
        IList<string> doorCodes = File.ReadLines("input.txt").ToList();

        KeypadEncoder npad = new(GetKeyPositions("789", "456", "123", " 0A"));
        KeypadEncoder dpad = new(GetKeyPositions(" ^A", "<v>"));

        int complexityTotal = 0;
        int dpadLevels = 2;
        
        foreach (string doorCode in doorCodes)
        {
            Console.WriteLine($"{0}: {doorCode}");

            IList<string> sequences = npad.EncodeOuterSequences([doorCode]);
            Console.WriteLine($"{1}: {sequences.First()} (x{sequences.Count})");
            for (int level = 1; level <= dpadLevels; ++level)
            {
                sequences = dpad.EncodeOuterSequences(sequences); // calculate the next level sequences
                Console.WriteLine($"{level+1}: {sequences.First()} (x{sequences.Count})");
            }

            // Sanity check: invert all encodings and compare with door code
            foreach (string sequence in sequences.Take(1))
            {
                string sequenceDecoded = sequence;
                for (int level = dpadLevels; level > 0; --level)
                {
                    sequenceDecoded = dpad.DecodeInnerSequence(sequenceDecoded);
                }
                string doorCodeDecoded = npad.DecodeInnerSequence(sequenceDecoded);
                Debug.Assert(doorCodeDecoded == doorCode);
            }

            int numericCode = int.Parse(doorCode.Trim('A'));
            int complexity = sequences.First().Length * numericCode;
            complexityTotal += complexity;

            Console.WriteLine($"[{sequences.First().Length} * {numericCode} = {complexity}]\n");
        }

        Console.WriteLine($"Total complexity: {complexityTotal}");
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

