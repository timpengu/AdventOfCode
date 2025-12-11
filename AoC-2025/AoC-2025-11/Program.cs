
using System.Collections.Immutable;

ILookup<string, string> nodes =
    File.ReadLines("input.txt")
    .SelectMany(s =>
    {
        string[] ss = s.Split([' ', ':'], StringSplitOptions.RemoveEmptyEntries).ToArray();
        return ss[1..].Select(ssi => (Key: ss[0], Value: ssi));
    })
    .ToLookup(v => v.Key, v => v.Value);


// part 1
var paths = Traverse("you", "out").ToList();
foreach (var path in paths)
{
    Console.WriteLine(string.Join(",", path));
}
Console.WriteLine(paths.Count);


// part 2
long pathsSvrDac = CountPathsDFS("svr", "dac");
long pathsSvrFft = CountPathsDFS("svr", "fft");
long pathsDacFft = CountPathsDFS("dac", "fft");
long pathsFftDac = CountPathsDFS("fft", "dac");
long pathsDacOut = CountPathsDFS("dac", "out");
long pathsFftOut = CountPathsDFS("fft", "out");

long pathsTotal =
    pathsSvrDac * pathsDacFft * pathsFftOut +
    pathsSvrFft * pathsFftDac * pathsDacOut;

Console.WriteLine(pathsTotal);

IEnumerable<IReadOnlyCollection<string>> Traverse(string from, string to) => TraverseDFS(from, to);

IEnumerable<IReadOnlyCollection<string>> TraverseBFS(string from, string to)
{
    Queue<ImmutableList<string>> queue = new();
    queue.Enqueue([from]);
    while (queue.TryDequeue(out var path))
    {
        var current = path.Last();
        if (current == to)
        {
            yield return path;
        }
        else
        {
            foreach (var next in nodes[current])
            {
                if (!path.Contains(next))
                {
                    queue.Enqueue(path.Add(next));
                }
            }
        }
    }
}

IEnumerable<IReadOnlyCollection<string>> TraverseDFS(string from, string to)
{
    return Traverse([from]);
    IEnumerable<IReadOnlyCollection<string>> Traverse(ImmutableList<string> path)
    {
        var current = path.Last();
        if (current == to)
        {
            yield return path;
        }
        else
        {
            foreach (var next in nodes[current])
            {
                if (!path.Contains(next))
                {
                    var nextPaths = Traverse(path.Add(next));
                    foreach (var nextPath in nextPaths)
                    {
                        yield return nextPath;
                    }
                }
            }
        }
    }
}

long CountPathsDFS(string from, string to)
{
    var memo = new Dictionary<string, long>();
    return Count([from]);

    long Count(ImmutableList<string> path)
    {
        var current = path.Last();
        if (current == to)
        {
            return 1L;
        }
        if (!memo.TryGetValue(current, out long count))
        {
            count = nodes[current]
                .Where(next => !path.Contains(next))
                .Sum(next => Count(path.Add(next)));

            memo[current] = count;
        }
        return count;
    }
}
