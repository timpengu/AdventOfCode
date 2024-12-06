
using System.Collections.Immutable;

IList<string> lines = File.ReadLines("input.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

int x = -1, y = -1;
IImmutableSet<(int, int)> blocks = ImmutableHashSet.Create<(int, int)>();

for (int ix = 0; ix < xs; ix++)
{
    for (int iy = 0; iy < ys; iy++)
    {
        switch (lines[iy][ix])
        {
            case '^':
                (x, y) = (ix, iy);
                break;

            case '#':
                blocks = blocks.Add((ix, iy));
                break;

            default:
                break;
        }
    }
}

(int Moves, int DistinctPositions, bool HasLooped) Patrol(IImmutableSet<(int, int)> blocks, int x, int y, int dx = 0, int dy = -1)
{
    bool IsInRange(int x, int y) => x >= 0 && x < xs && y >= 0 && y < ys;

    bool hasLooped = false;
    int move = 0;
    HashSet<(int X, int Y, int dX, int dY)> statesVisited = new();
    while (IsInRange(x, y))
    {
        if (!statesVisited.Add((x, y, dx, dy)))
        {
            hasLooped = true;
            break;
        }

        // Console.WriteLine($"\nMove {statesVisited.Count}:");
        // ConsoleWriteMap(blocks, x, y, dx, dy);
        // Console.WriteLine();

        (int x2, int y2) = (x + dx, y + dy); // move forward
        
        while (blocks.Contains((x2,y2)))
        {
            (dx, dy) = (-dy, dx); // rotate direction vector
            (x2, y2) = (x + dx, y + dy); // turn right
        }

        (x, y) = (x2, y2);
    }

    int distinctPositions = statesVisited.DistinctBy(s => (s.X, s.Y)).Count();
    return (statesVisited.Count, distinctPositions, hasLooped);
}

void ConsoleWriteMap(IImmutableSet<(int, int)> blocks, int x, int y, int dx = 0, int dy = -1)
{
    for (int iy = 0; iy < ys; iy++)
    {
        for (int ix = 0; ix < xs; ix++)
        {
            Console.Write(
                (ix, iy) == (x, y)
                    ? (dx, dy) switch
                                {
                                    (0, -1) => '^',
                                    (+1, 0) => '>',
                                    (0, +1) => 'v',
                                    (-1, 0) => '<',
                                    _ => '@'
                                }
                    : blocks.Contains((ix, iy)) ? '#' : '.'
            );
        }
        Console.WriteLine();
    }
}

// ConsoleWriteMap(blocks, x, y);

// Part 1
{
    var result = Patrol(blocks, x, y);
    Console.WriteLine($"\nMoves: {result.Moves}");
    Console.WriteLine($"Distinct positions: {result.DistinctPositions}");
}

// Part 2
{
    int loopingSolutions = 0;
    for (int iy = 0; iy < ys; iy++)
    {
        for (int ix = 0; ix < xs; ix++)
        {
            if ((ix,iy) == (x,y) || blocks.Contains((ix, iy)))
            {
                continue;
            }

            var withExtraBlock = blocks.Add((ix, iy));
            var result = Patrol(withExtraBlock, x, y);
            if (result.HasLooped)
            {
                ++loopingSolutions;

                Console.WriteLine($"\nWith extra block @ ({ix},{iy}):");
                Console.WriteLine($"Looped after {result.Moves} moves");
                Console.WriteLine($"Distinct positions: {result.DistinctPositions}");
                // ConsoleWriteMap(withExtraBlock, x, y);
            }
        }
    }

    Console.WriteLine($"\nLooping solutions: {loopingSolutions}");
}
