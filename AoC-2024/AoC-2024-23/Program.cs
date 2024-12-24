using System.Collections.Immutable;
using System.Diagnostics;

List<(string A, string B)> edges = new(
    File.ReadLines("input.txt").Select(line =>
    {
        string[] items = line.Split('-', 2);
        Debug.Assert(items.Length == 2 && items.All(s => s.Length == 2));
        return (items[0], items[1]);
    }));

//Comparer.Create()
List<string[]> cliques3 = FindCliques3(edges).ToList();
foreach (var clique in cliques3)
{
    Console.WriteLine($"{String.Join(',', clique)}");
}

int tNodeCliques = cliques3.Count(c => c.Any(n => n.StartsWith('t')));

Console.WriteLine($"Total 3-cliques: {cliques3.Count}");
Console.WriteLine($"3-cliques with T nodes: {tNodeCliques}");

Dictionary<string, int> memoizedCliqueMax = new();

Stopwatch sw = Stopwatch.StartNew();
string[] maxClique = FindMaxCliqueBK(edges);
sw.Stop();
Console.WriteLine($"Largest clique: {String.Join(',', maxClique)} (size {maxClique.Length}) [{sw.Elapsed}]");

IEnumerable<string[]> FindCliques3(IList<(string, string)> edges)
{
    Dictionary<string, List<string>> connectedNodes = GetConnectedNodes(edges);

    foreach (var kvp in connectedNodes)
    {
        string node0 = kvp.Key;
        List<string> firstNeighbours = kvp.Value;

        foreach (var node1 in firstNeighbours)
        {
            connectedNodes[node1].Remove(node0); // remove node1->node0 edge

            var secondNeighbours = connectedNodes[node1];
            var clique3Nodes = secondNeighbours.Where(node2 => connectedNodes[node2].Contains(node0));
            foreach(var node2 in clique3Nodes)
            {
                yield return [node0, node1, node2];
            }
        }

        connectedNodes.Remove(node0); // remove node0->node1 edges
    }
}

string[] FindMaxCliqueTP(IList<(string, string)> edges)
{
    Dictionary<string, List<string>> connectedNodes = GetConnectedNodes(edges);
    HashSet<string> visitedCliques = new();
    List<IImmutableSet<string>> maximalCliques = new();

    // extend cliques from each individual node
    foreach (string node in connectedNodes.Keys)
    {
        TimKibosh(ImmutableHashSet.Create(node));
    }

    IImmutableSet<string> maxClique = maximalCliques.OrderByDescending(clique => clique.Count).First();
    return maxClique.OrderBy(s => s).ToArray();

    // Extends a given clique recursively to find all maximal supercliques
    // My own crappy "intuitive" algorithm
    void TimKibosh(IImmutableSet<string> clique)
    {
        string key = String.Join(',', clique.OrderBy(s => s)); // HACK: slow
        if (!visitedCliques.Add(key))
        {
            return; // already visited this clique, don't repeat work
        }
            
        List<string> extendingNodes = clique
            .SelectMany(cliqueNode => connectedNodes[cliqueNode]) // all nodes connected to this clique
            .Where(node => !clique.Contains(node)) // that are not in the clique
            .Where(node => connectedNodes[node].Count(clique.Contains) == clique.Count) // but connected to all nodes in the clique
            .ToList();

        if (!extendingNodes.Any())
        {
            // cannot extend this clique further
            maximalCliques.Add(clique);
            Console.WriteLine($"Maximal clique: {key} (size {clique.Count})");
            return;
        }

        foreach (var node in extendingNodes)
        {
            TimKibosh(clique.Add(node)); // extend the clique and recurse
        }
    }
}

string[] FindMaxCliqueBK(IList<(string, string)> edges)
{
    Dictionary<string, List<string>> connectedNodes = GetConnectedNodes(edges);
    List<IImmutableSet<string>> maximalCliques = new();

    BronKerbosch(
        ImmutableHashSet.Create<string>(),
        ImmutableHashSet.Create(connectedNodes.Keys.ToArray()),
        ImmutableHashSet.Create<string>());

    IImmutableSet<string> maxClique = maximalCliques.OrderByDescending(clique => clique.Count).First();
    return maxClique.OrderBy(s => s).ToArray();

    // Finds all maximal cliques recursively
    // https://en.wikipedia.org/wiki/Bron%E2%80%93Kerbosch_algorithm
    void BronKerbosch(IImmutableSet<string> r, IImmutableSet<string> p, IImmutableSet<string> x)
    {
        if (p.Count == 0 && x.Count == 0)
        {
            maximalCliques.Add(r);
            Console.WriteLine($"Maximal clique: {String.Join(',', r.Order())} (size {r.Count})");
        }

        foreach (string node in p)
        {
            List<string> neighbourNodes = connectedNodes[node];
            
            BronKerbosch(
                r.Add(node),
                p.Intersect(neighbourNodes),
                x.Intersect(neighbourNodes));

            p = p.Remove(node);
            x = x.Add(node);
        }
    }
}

Dictionary<string, List<string>> GetConnectedNodes(IList<(string, string)> edges) => edges
    .Select(e => (e.Item2, e.Item1)) // add commuted pairs
    .Concat(edges)
    .GroupBy(e => e.Item1, e => e.Item2)
    .ToDictionary(
        g => g.Key,
        g => g.Distinct().ToList());
