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

Stopwatch sw = Stopwatch.StartNew();

// part 1
List<string> matchedDesigns = new(designs
    .OrderBy(design => design)
    .Where(design => GetMatches(design, patterns).Any()) // just find a single solution for each design 
);
Console.WriteLine($"\nPossible designs: {matchedDesigns.Count} @ {sw.Elapsed}");

// part 2
Console.WriteLine($"\n{"Design",-60} {"Combinations",20}");
long totalMatches = 0;
foreach (var design in matchedDesigns)
{
    long matches = CountMatches(design, patterns);
    totalMatches += matches;

    Console.WriteLine($"{design,-60} {matches,20}");
}
Console.WriteLine($"\nTotal matching combinations: {totalMatches} @ {sw.Elapsed}");

IEnumerable<IList<string>> GetMatches(string toMatch, IList<string> patterns)
{
    return Match(0, ImmutableStack<string>.Empty);

    // DFS to return all matches without memoisation (won't work for large inputs!)
    IEnumerable<IList<string>> Match(int matchIndex, IImmutableStack<string> pattenSequence)
    {
        Debug.Assert(matchIndex <= toMatch.Length);

        return matchIndex == toMatch.Length
            ? [pattenSequence.Reverse().ToList()]
            : patterns
                .Where(p => toMatch.AsSpan(matchIndex).StartsWith(p))
                .SelectMany(p => Match(matchIndex + p.Length, pattenSequence.Push(p)));
    }
}

long CountMatches(string toMatch, IList<string> patterns)
{
    long[] memoMatches = Enumerable
        .Repeat(-1L, toMatch.Length) // indexes [0..Length) are not yet visited, indicated by -1
        .Append(1L)                  // EOL means matched whole string so there are exactly 1 matches
        .ToArray();                  // use array instead of Dictionary<int,long>

    return CountMatches(0);

    // DFS with memoisation of matches in the unmatched portion (since it is independent of patterns matched so far)
    long CountMatches(int matchIndex)
    {
        Debug.Assert(matchIndex <= toMatch.Length);

        long matches = memoMatches[matchIndex]; // already calculated matches for the remaining string length?
        if (matches < 0)
        {
            matches = patterns
                .Where(p => toMatch.AsSpan(matchIndex).StartsWith(p))
                .Select(p => CountMatches(matchIndex + p.Length))
                .Sum();

            memoMatches[matchIndex] = matches;
        }

        return matches;
    }
}
