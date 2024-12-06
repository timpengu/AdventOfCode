using MoreLinq;
using System.Collections.Immutable;

int verboseLevel = 0; // 0..2

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

Coord zInit = (-1, -1);
Coord dzInit = (0, -1);

IImmutableSet<Coord> blocks = ImmutableHashSet.Create<Coord>();

for (int iy = 0; iy < ys; iy++)
{
    for (int ix = 0; ix < xs; ix++)
    {
        switch (lines[iy][ix])
        {
            case '^':
                zInit = (ix, iy);
                break;

            case '#':
                blocks = blocks.Add((ix, iy));
                break;

            default:
                break;
        }
    }
}

// Part 1
var patrol = Patrol(blocks, zInit, dzInit);

Console.WriteLine($"Left map after {patrol.Moves.Count} moves");
Console.WriteLine($"Distinct positions visited: {CountDistinctPositions(patrol.Moves)}");
if (verboseLevel >= 2)
{
    ConsoleWriteMap(blocks, patrol.Moves);
}
Console.WriteLine();

// Part 2
int loopingSolutions = 0;
foreach (Coord z in DistinctPositions(patrol.Moves).Where(z => z != zInit)) // exclude starting position
{
    IImmutableSet<Coord> withExtraBlock = blocks.Add(z);
    
    var blockedPatrol = Patrol(withExtraBlock, zInit, dzInit);
    if (blockedPatrol.HasLooped)
    {
        ++loopingSolutions;

        if (verboseLevel >= 1)
        {
            Console.WriteLine($"With extra block @ {z}:");
            Console.WriteLine($"Looped after {blockedPatrol.Moves.Count} moves");
            Console.WriteLine($"Distinct positions: {CountDistinctPositions(blockedPatrol.Moves)}");

            if (verboseLevel >= 2)
            {
                ConsoleWriteMap(withExtraBlock, blockedPatrol.Moves, z);
            }

            Console.WriteLine();
        }
    }
}

Console.WriteLine($"Looping solutions with one extra block: {loopingSolutions}");

bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;

int CountDistinctPositions(IEnumerable<(Coord Z, Coord dZ)> moves) => DistinctPositions(moves).Count();
IEnumerable<Coord> DistinctPositions(IEnumerable<(Coord Z, Coord dZ)> moves) => moves.Select(m => m.Z).Distinct();

(IReadOnlyCollection<(Coord Z, Coord dZ)> Moves, bool HasLooped) Patrol(IImmutableSet<Coord> blocks, Coord z, Coord dz)
{
    HashSet<(Coord Z, Coord dZ)> visited = new(); // hash set of moves to detect cycles
    List<(Coord Z, Coord dZ)> moves = new(); // ordered list of moves (unfortunately no ordered hash set in .NET)
    bool hasLooped = false;

    while (IsInRange(z))
    {
        moves.Add((z, dz));

        if (!visited.Add((z, dz))) // already been on this path?
        {
            hasLooped = true;
            break;
        }

        while (blocks.Contains(z + dz)) // next position is blocked?
        {
            dz = (-dz.Y, dz.X); // rotate direction vector
        }

        z += dz; // move in current direction
    }

    return (moves, hasLooped);
}

void ConsoleWriteMap(IImmutableSet<Coord> blocks, IReadOnlyCollection<(Coord Z, Coord dZ)> moves, Coord? zExtraBlock = null)
{
    Coord zStart = moves.Any() ? moves.First().Z : (-1, -1);
    IDictionary<Coord, (bool X, bool Y)> hasMoves = moves
        .Concat(moves.Pairwise((m1, m2) => (m1.Z, m2.dZ))) // include next direction when it changes 
        .GroupBy(m => m.Z, m => m.dZ)
        .ToDictionary(
            g => g.Key,
            g => (HasX: g.Any(dz => dz.X != 0), HasY: g.Any(dz => dz.Y != 0)));

    for (int iy = 0; iy < ys; iy++)
    {
        for (int ix = 0; ix < xs; ix++)
        {
            Coord z = (ix, iy);

            hasMoves.TryGetValue(z, out (bool X, bool Y) hasMove);

            Console.Write(
                z == zStart ? '^' :
                z == zExtraBlock ? 'O' :
                blocks.Contains(z) ? '#' :
                (hasMove.X, hasMove.Y) switch
                {
                    (true, false) => '-',
                    (false, true) => '|',
                    (true, true) => '+',
                    _ => '.'
                });
        }

        Console.WriteLine();
    }
}
