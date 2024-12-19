using System.Collections.Immutable;
using System.Diagnostics;

List<string> patterns = new();
List<string> designs = new();

using (StreamReader file = new("input.txt"))
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

Console.WriteLine($"Patterns: {patterns.Count} \tMin,Max,Avg length: {patterns.Min(p => p.Length)}, {patterns.Max(p => p.Length)}, {patterns.Average(p => p.Length)}");
Console.WriteLine($"Designs:  {designs.Count}  \tMin,Max,Avg length: {designs.Min(p => p.Length)}, {designs.Max(p => p.Length)}, {designs.Average(p => p.Length)}");

// part 1
List<string> matchedDesigns = new(designs
    .OrderBy(design => design)
    .Where(design => GetMatches(design, patterns).Any()) // just find a single solution for each design 
);
Console.WriteLine($"\nPossible designs: {matchedDesigns.Count}");

// part 2
Console.WriteLine($"\n{"Design",-60} {"Combinations",20}");
long totalMatches = 0;
foreach (var design in matchedDesigns)
{
    long matches = CountMatches(design, patterns);
    totalMatches += matches;

    Console.WriteLine($"{design,-60} {matches,20}");
}
Console.WriteLine($"\nTotal matching combinations: {totalMatches}");

long CountMatches(string toMatch, IList<string> patterns)
{
    ILookup<char, int> patternLookup = GetLookupByFirstChar(patterns);

    long[] memoMatches = Enumerable
        .Repeat(-1L, toMatch.Length) // indexes [0..Length) are not yet visited, indicated by -1
        .Append(1L)                  // EOL means matched whole string so there are exactly 1 matches
        .ToArray();                  // use array instead of Dictionary<int,long>

    return CountMatches(0, ImmutableStack<int>.Empty);

    // DFS with memoisation of matches in the unmatched portion (since it is independent of patterns matched so far)
    long CountMatches(int matchIndex, IImmutableStack<int> patternIndexSequence)
    {
        Debug.Assert(matchIndex <= toMatch.Length);

        long matches = memoMatches[matchIndex]; // already calculated matches for the remaining string length?
        if (matches < 0)
        {
            matches = Enumerable.Sum(
                from index in patternLookup[toMatch[matchIndex]]
                let pattern = patterns[index]
                where toMatch.AsSpan(matchIndex).StartsWith(pattern)
                select CountMatches(matchIndex + pattern.Length, patternIndexSequence.Push(index))
            );

            memoMatches[matchIndex] = matches;
        }

        return matches;
    }
}

IEnumerable<IList<int>> GetMatches(string toMatch, IList<string> patterns)
{
    ILookup<char, int> patternLookup = GetLookupByFirstChar(patterns);

    return Match(0, ImmutableStack<int>.Empty);

    // DFS to return all matches without memoisation (won't work for large inputs!)
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

ILookup<char,int> GetLookupByFirstChar(IEnumerable<string> patterns) => patterns
    .Select((p, i) => (FirstChar: p[0], Index: i))
    .ToLookup(v => v.FirstChar, v => v.Index);
