using MoreLinq;
using System.Diagnostics;

internal static class Program
{
    private static void Main(string[] args)
    {
        IList<string> doorCodes = File.ReadLines("input.txt").ToList();

        KeypadEncoder npad = new(GetKeyPositions("789", "456", "123", " 0A"));
        KeypadEncoder dpad = new(GetKeyPositions(" ^A", "<v>"));

        int dpadLevelsPart1 = 2;
        int dpadLevelsPart2 = 25;

        long complexityTotalPart1 = 0;
        long complexityTotalPart2 = 0;

        foreach (string doorCode in doorCodes)
        {
            Console.WriteLine($"{0}: {doorCode}");

            IList<string> sequences = npad.EncodeOuterSequences([doorCode]);
            Console.WriteLine($"{1}: {sequences.First()} (x{sequences.Count})");

            for (int level = 1; level <= dpadLevelsPart1; ++level)
            {
                sequences = dpad.EncodeOuterSequences(sequences); // calculate the next level sequences
                Console.WriteLine($"{level+1}: {sequences.First()} (x{sequences.Count})");
            }

            int numericCode = int.Parse(doorCode.Trim('A'));
            long expandedLengthPart1 = sequences.First().Length;
            long complexityPart1 = expandedLengthPart1 * numericCode;
            Console.WriteLine($"[{expandedLengthPart1} * {numericCode} = {complexityPart1}]");

            long expandedLengthPart2 = sequences.Min(seq => dpad.GetEncodedOuterSequenceMinLength(seq, dpadLevelsPart2));
            Console.WriteLine("...");
            Console.WriteLine($"{dpadLevelsPart2 + 1}: Minimum expanded length: {expandedLengthPart2}");

            long complexityPart2 = expandedLengthPart2 * numericCode;

            Console.WriteLine($"[{expandedLengthPart2} * {numericCode} = {complexityPart2}]\n");

            complexityTotalPart1 += complexityPart1;
            complexityTotalPart2 += complexityPart2;
        }

        Console.WriteLine($"Total complexity after {dpadLevelsPart1} encodings: {complexityTotalPart1}");
        Console.WriteLine($"Total complexity after {dpadLevelsPart2} encodings: {complexityTotalPart2}");
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

