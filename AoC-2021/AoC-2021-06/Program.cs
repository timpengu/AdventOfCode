
const int RepoInterval = 7;
const int RepoMaturity = 9;

List<int> values =
    File.ReadLines("input.txt")
    .Single()
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList();

long[] dist =
    Enumerable.Range(0, RepoMaturity)
    .Select(x => (long)values.Count(v => v == x))
    .ToArray();

Console.WriteLine($"Initial state: [{dist.Sum()}] {String.Join(',', dist)}");

for (int day = 1; day <= 256; ++day)
{
    AdvanceDay(dist);

    Console.WriteLine($"After {day} days: [{dist.Sum()}] {String.Join(',', dist)}");
}

void AdvanceDay(long[] dist)
{
    long matureCount = dist[0];
    for(int i = 1; i < RepoMaturity; ++i)
    {
        dist[i - 1] = dist[i];
    }
    dist[RepoInterval - 1] += matureCount;
    dist[RepoMaturity - 1] = matureCount;
}