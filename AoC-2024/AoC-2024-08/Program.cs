IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

List<Antenna> antennas = new(
    from iy in Enumerable.Range(0, ys)
    from ix in Enumerable.Range(0, xs)
    let c = lines[iy][ix]
    where char.IsLetterOrDigit(c)
    select new Antenna(c, (ix, iy))
);

ISet<Coord> antiNodes1 = GetAntiNodes([1]);

ConsoleWriteMap(antiNodes1);
Console.WriteLine($"\nOrder-1 AntiNodes: {antiNodes1.Count}\n");

ISet<Coord> antiNodesN = GetAntiNodes(Enumerable.Range(0, int.MaxValue));

ConsoleWriteMap(antiNodesN);
Console.WriteLine($"\nOrder-N AntiNodes: {antiNodesN.Count}\n");


ISet<Coord> GetAntiNodes(IEnumerable<int> generateOrders) => new HashSet<Coord>(
    from a in antennas
    from b in antennas
    where a.Sign == b.Sign && a.Z != b.Z // for each ordered pair of antennas with the same sign
    from antiNode in generateOrders.Select(o => GetAntiNode(a, b, o)).TakeWhile(IsInRange)
    select antiNode);

Coord GetAntiNode(Antenna a, Antenna b, int order = 1) => a.Z + order * (a.Z - b.Z);
bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;

void ConsoleWriteMap(ISet<Coord> antiNodes)
{
    IDictionary<Coord, char> antennasByPos = antennas.ToDictionary(a => a.Z, a => a.Sign);

    for (int y = 0; y < ys; ++y)
    {
        for (int x = 0; x < xs; ++x)
        {
            Coord z = (x, y);

            bool hasAntiNode = antiNodes.Contains(z);
            bool hasAntenna = antennasByPos.TryGetValue(z, out char sign);

            Console.BackgroundColor = hasAntiNode ? ConsoleColor.DarkRed : ConsoleColor.Black;
            Console.ForegroundColor = hasAntenna ? ConsoleColor.Green : ConsoleColor.White;
            Console.Write(hasAntenna ? sign : hasAntiNode ? '#' : '.');
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine();
    }
}
