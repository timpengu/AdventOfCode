
List<(int Before, int After)> pageOrders = new();
List<List<int>> pageLists = new();

using (StreamReader file = new("input.txt"))
{
    // Read page orders "{a}|{b}"
    for (string? line = file.ReadLine(); line?.Length > 0; line = file.ReadLine())
    {
        List<int> pageOrder = line.Split('|', 2).Select(int.Parse).ToList();
        pageOrders.Add((pageOrder[0], pageOrder[1]));
    }

    // Read page lists "{a},{b},{c}..."
    for (string? line = file.ReadLine(); line?.Length > 0; line = file.ReadLine())
    {
        List<int> pageList = line.Split(',').Select(int.Parse).ToList();
        pageLists.Add(pageList);
    }
}

// Precompute ordering lookup for efficiency
ILookup<int, int> pagesMustComeAfter = pageOrders.ToLookup(po => po.Before, po => po.After);

bool IsOrdered(IList<int> pages) => !IsDisordered(pages);
bool IsDisordered(IList<int> pages) => GetConflicts(pages).Any();
int SelectMiddlePage(IList<int> pages) => pages[pages.Count / 2];

IEnumerable<(int PageBefore, int PageAfter)> GetConflicts(IList<int> pages) =>
    from thisPage in pages.Skip(1)
    from prevPage in pages.TakeWhile(page => page != thisPage) // assume distinct pages list (avoids indexes)
    where pagesMustComeAfter[thisPage].Contains(prevPage)
    select (prevPage, thisPage);

// Use a version of Kahn's algorithm to perform topological sort:
IList<int> Reorder(IList<int> pages)
{
    // Get edges between pages in the list to sort
    List<(int Before, int After)> edges = pages.SelectMany(p1 => pagesMustComeAfter[p1], (p1, p2) => (p1, p2)).ToList();

    List<int> orderedPages = new();
    List<int> nextPages;
    do
    {
        // Add remaining pages with no incoming edges (i.e. not after any other remaining pages)
        nextPages = pages.Except(orderedPages).Where(page => !edges.Any(e => e.After == page)).ToList();       
        orderedPages.AddRange(nextPages);
        edges.RemoveAll(e => nextPages.Contains(e.Before));
    }
    while (nextPages.Any());

    return orderedPages;
}

// Can also implement this very simply just using LINQ with custom comparer:
IList<int> Reorder2(IList<int> pages) => pages
    .Order(Comparer<int>.Create((a, b) =>
        // It would be simpler just to use a HashSet<(int,int)> for this purpose
        pagesMustComeAfter[a].Contains(b) ? +1 :
        pagesMustComeAfter[b].Contains(a) ? -1 : 0))
    .ToList();

foreach (List<int> pageList in pageLists)
{
    Console.WriteLine($"PageList: {String.Join(",", pageList)}");

    List<(int PageBefore, int PageAfter)> conflicts = GetConflicts(pageList).ToList();
    if (conflicts.Any())
    {
        Console.WriteLine($"Conflicts: {String.Join(" ", conflicts.Select(c => $"({c.PageBefore},{c.PageAfter})"))}");
        Console.WriteLine($"Reordered: {String.Join(",", Reorder(pageList))}");
    }
    else
    {
        Console.WriteLine($"Conflicts: None");
    }

    Console.WriteLine();
}

int sumOfOrderedMiddlePages = pageLists.Where(IsOrdered).Sum(SelectMiddlePage);
Console.WriteLine(sumOfOrderedMiddlePages);

int sumOfReorderedMiddlePages = pageLists.Where(IsDisordered).Select(Reorder).Sum(SelectMiddlePage);
int sumOfReorderedMiddlePages2 = pageLists.Where(IsDisordered).Select(Reorder2).Sum(SelectMiddlePage);
Console.WriteLine($"{sumOfReorderedMiddlePages} {sumOfReorderedMiddlePages2}");
