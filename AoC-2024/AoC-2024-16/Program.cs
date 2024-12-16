
using MoreLinq;
using System.Collections.Immutable;

Coord[] Directions = [(0, +1), (+1, 0), (0, -1), (-1, 0)];

IList<string> lines = File.ReadLines("inputSample.txt").ToList();
int xs = lines.Select(x => x.Length).Distinct().Single();
int ys = lines.Count;

(bool[,] isWall, Coord zStart, Coord zEnd) = Parse(lines);

var results =
    from path in FindPaths(zStart, zEnd)
    let metrics = CalcPathMetrics(path)
    orderby metrics.Score
    select (Path:path, metrics.Score, metrics.Steps, metrics.Turns);

var result = results.First();
//foreach (var result in results.Take(10))
{
    Console.WriteLine($"Score:{result.Score} Steps:{result.Steps} Turns:{result.Turns} Path:{String.Join("", result.Path)}");
}

(int Steps, int Turns, int Score) CalcPathMetrics(IList<Coord> path)
{
    int steps = path.Count - 1;
    int turns = path
        .Pairwise((z1, z2) => z2 - z1)
        .Prepend((+1, 0)) // start facing east
        .Pairwise((dz1, dz2) => (dz1, dz2))
        .Count(step => step.dz1 != step.dz2);

    return (steps, turns, steps + turns * 1000);
}

IEnumerable<IList<Coord>> FindPaths(Coord zStart, Coord zTarget) => ExtendPath(ImmutableStack.Create(zStart), zTarget);
IEnumerable<IList<Coord>> ExtendPath(IImmutableStack<Coord> path, Coord zTarget)
{
    Coord z = path.Peek();
    Coord dz = path.Take(2).Pairwise((z2, z1) => z2 - z1).FallbackIfEmpty((+1, 0)).Single();
    Coord[] directions = [dz, (-dz.Y, dz.X), (dz.Y, -dz.X)]; // (-dz.X, -dz.Y) go back? (can't go west at start)

    return z == zTarget
        ? Enumerable.Repeat(path.Reverse().ToList(), 1) // return this path
        : directions
            .Select(dz => z + dz)
            .Where(IsPath)
            .Where(zNext => !path.Contains(zNext)) // avoid cycles
            .SelectMany(zNext => ExtendPath(path.Push(zNext), zTarget)); // extend path recursively
}

IEnumerable<Coord> GetNeighbours(Coord z) => Directions.Select(dz => z + dz);
bool IsPath(Coord z) => IsInRange(z) && !isWall[z.X, z.Y];
bool IsInRange(Coord z) => z.X >= 0 && z.X < xs && z.Y >= 0 && z.Y < ys;

(bool[,], Coord, Coord) Parse(IList<string> lines)
{
    bool[,] isWall = new bool[xs, ys];
    Coord? start = null, end = null;
    foreach (int x in Enumerable.Range(0, xs))
    {
        foreach (int y in Enumerable.Range(0, ys))
        {
            char c = lines[y][x];
            switch (c)
            {
                case '.' or '#':
                    isWall[x, y] = (c == '#');
                    break;
                case 'S':
                    start = (x, y);
                    break;
                case 'E':
                    end = (x, y);
                    break;
                default:
                    throw new InvalidDataException($"Invalid input character: '{c}'");
            }
        }
    }
    return (
        isWall,
        start ?? throw new InvalidDataException("No start position"),
        end ?? throw new InvalidDataException("No end position")
    );
}

record struct Coord(int X, int Y)
{
    public static implicit operator Coord((int X, int Y) tuple) => new Coord(tuple.X, tuple.Y);
    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new Coord(a.X - b.X, a.Y - b.Y);
    public override string ToString() => $"({X},{Y})";
}
