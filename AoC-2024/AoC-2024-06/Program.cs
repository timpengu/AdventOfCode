
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
{
    var patrol = Patrol(blocks, zInit, dzInit);

    Console.WriteLine($"Left map after {patrol.Moves.Count} moves");
    Console.WriteLine($"Distinct positions visited: {CountDistinctPositions(patrol.Moves)}");
    if (verboseLevel >= 2)
    {
        ConsoleWriteMap(blocks, patrol.Moves);
    }
    Console.WriteLine();
}

// Part 2
{
    int loopingSolutions = 0;
    for (int iy = 0; iy < ys; iy++)
    {
        for (int ix = 0; ix < xs; ix++)
        {
            Coord z = (ix, iy);

            if (z == zInit || blocks.Contains(z))
            {
                continue; // don't place a block on top of the guard or existing blocks
            }

            IImmutableSet<Coord> withExtraBlock = blocks.Add(z);

            var patrol = Patrol(withExtraBlock, zInit, dzInit);
            if (patrol.HasLooped)
            {
                ++loopingSolutions;

                if (verboseLevel >= 1)
                {
                    Console.WriteLine($"With extra block @ {z}:");
                    Console.WriteLine($"Looped after {patrol.Moves.Count} moves");
                    Console.WriteLine($"Distinct positions: {CountDistinctPositions(patrol.Moves)}");

                    if (verboseLevel >= 2)
                    {
                        ConsoleWriteMap(withExtraBlock, patrol.Moves, z);
                    }

                    Console.WriteLine();
                }
            }
        }
    }

    Console.WriteLine($"Looping solutions with one extra block: {loopingSolutions}");
}

bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;
int CountDistinctPositions(IEnumerable<(Coord Z, Coord dZ)> moves) => moves.DistinctBy(s => s.Z).Count();

(IReadOnlyCollection<(Coord Z, Coord dZ)> Moves, bool HasLooped) Patrol(IImmutableSet<Coord> blocks, Coord z, Coord dz)
{
    List<(Coord Z, Coord dZ)> moves = new(); // ordered list of moves
    HashSet<(Coord Z, Coord dZ)> visited = new(); // hash set of moves for loop check (unfortunately no ordered hash set in .NET)

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
    ILookup<Coord, Coord> directionsByPosition = moves
        .Concat(moves.Pairwise((m1, m2) => (m1.Z, m2.dZ))) // include new direction if it changes
        .ToLookup(m => m.Z, m => m.dZ);

    for (int iy = 0; iy < ys; iy++)
    {
        for (int ix = 0; ix < xs; ix++)
        {
            Coord z = (ix, iy);

            List<Coord> dirs = directionsByPosition[z].ToList();
            bool hasMoveH = dirs.Any(dz => dz.X != 0);
            bool hasMoveY = dirs.Any(dz => dz.Y != 0);

            Console.Write(
                z == zStart ? '^' :
                z == zExtraBlock ? 'O' :
                blocks.Contains(z) ? '#' :
                (hasMoveH, hasMoveY) switch
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
