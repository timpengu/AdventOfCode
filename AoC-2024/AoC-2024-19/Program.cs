
using System.Collections.Immutable;
using System.Diagnostics;

List<string> patterns = new();
List<string> designs = new();

using (StreamReader file = new("inputSample.txt"))
{
    patterns.AddRange(
        file.ReadLine()
        !.Split(",", StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim()));

    designs = new();
    while (!file.EndOfStream)
    {
        string? line = file.ReadLine();
        if (line?.Length > 0)
        {
            designs.Add(line);
        }
    }
}

Debug.Assert(patterns.Count > 0);
Debug.Assert(patterns.All(s => s.Length > 0));
Debug.Assert(designs.Count > 0);
Debug.Assert(designs.All(s => s.Length > 0));

Console.WriteLine($"Patterns: {patterns.Count} \tAverage length: {patterns.Average(p => p.Length)}");
Console.WriteLine($"Designs:  {designs.Count}  \tAverage length: {designs.Average(p => p.Length)}");

// part 1
int matchedDesigns = designs.Count(design => Match(design, patterns).Any());
Console.WriteLine($"\nMatching designs: {matchedDesigns}");

// part 2
int totalMatches = 0;
foreach (var design in designs)
{
    IList<IList<int>> matches = Match(design, patterns).ToList();
    totalMatches += matches.Count;

    Console.WriteLine($"\nMatch '{design}' combinations: {matches.Count}");
    foreach (var match in matches)
    {
        string combo = String.Join(" ", match.Select(i => patterns[i]));
        Console.WriteLine(combo);
    }
}
Console.WriteLine($"\nTotal matching combinations: {totalMatches}");

IEnumerable<IList<int>> Match(string toMatch, IList<string> patterns)
{
    ILookup<char, int> patternLookup = patterns
        .Select((p, i) => (FirstChar: p[0], Index: i))
        .ToLookup(v => v.FirstChar, v => v.Index);

    return Match(0, ImmutableStack<int>.Empty);

    IEnumerable<IList<int>> Match(int matchIndex, IImmutableStack<int> patternIndexSequence)
    {
        Debug.Assert(matchIndex <= toMatch.Length);

        return matchIndex == toMatch.Length
            ? [ patternIndexSequence.Reverse().ToList() ]
            : from index in patternLookup[toMatch[matchIndex]]
              let pattern = patterns[index]
              where toMatch.AsSpan(matchIndex).StartsWith(pattern)
              from result in Match(matchIndex + pattern.Length, patternIndexSequence.Push(index))
              select result;
    }
}

