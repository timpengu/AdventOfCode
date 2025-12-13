using AoC_2025_12.GameModel;
using AoC_2025_12.GameModel.Coords;
using AoC_2025_12.GameModel.Game;
using System.Text.RegularExpressions;

List<Piece> pieces = [];
List<Region> regions = [];

using (StreamReader file = new("input.txt"))
{
    while (!file.EndOfStream)
    {
        string? s = file.ReadLine()?.Trim();
        if (String.IsNullOrEmpty(s)) continue;

        if (s.TryParseShapeHeader(out int id))
        {
            bool[,] cells = file.ReadShapeCells();
            var piece = BuildPiece(id, cells);
            pieces.Add(piece);
        }
        else if (s.TryParseArea(out Coord size, out int[] pieceCounts))
        {
            Region region = new(size, pieceCounts);
            regions.Add(region);
        }
        else
        {
            throw new Exception($"Invalid input: {s}");
        }
    }
}

var stats = regions.Select(r => (
    Places: (r.Size.X / 3) * (r.Size.Y / 3),
    Pieces: r.PieceCounts.Values.Sum(), 
    BoardPositions: r.Size.X * r.Size.Y,
    PiecePositions: r.PieceCounts.Values.Sum() * 9
)).ToList();

int underConstrained = stats.Count(s => s.PiecePositions <= s.BoardPositions);
int underPlaced = stats.Count(s => s.Pieces <= s.Places);

double meanUnderConstraint = stats.Where(s => s.PiecePositions <= s.BoardPositions).Average(s => (double)s.PiecePositions/s.BoardPositions);
double meanOverConstraint = stats.Where(s => s.PiecePositions > s.BoardPositions).Average(s => (double)s.PiecePositions / s.BoardPositions);

double meanUnderPlaced = stats.Where(s => s.Pieces <= s.Places).Average(s => (double)s.Pieces/ s.Places);
double meanOverPlaced = stats.Where(s => s.Pieces > s.Places).Average(s => (double)s.Pieces / s.Places);

Console.WriteLine($"Under constrained:{underConstrained} MeanUnder:{meanUnderConstraint} MeanOver:{meanOverConstraint}");
Console.WriteLine($"Under placed: {underPlaced} MeanUnder:{meanUnderPlaced} MeanOver:{meanOverPlaced}");

int solutions = 0;
for (int r = 0; r < regions.Count; ++r)
{
    var region = regions[r];
    var board = Board.Create(region.Size);

    List<Piece> piecesOnBoard = pieces
        .Select(piece => (Piece: piece, Count: region.PieceCounts[piece.Name]))
        .SelectMany(p => Enumerable.Repeat(p.Piece, p.Count))
        .ToList();

    int boardPositions = board.Bounds.EnumerateCoords().Count(coord => !board.IsOccupied(coord));
    int piecePositions = piecesOnBoard.Sum(p => p.PositionCount);
    int maxPiecePositions = piecesOnBoard.Count * 9;

    Solution? solution = piecePositions <= boardPositions
        ? Solver.GetSolutions(board, piecesOnBoard).Cast<Solution?>().FirstOrDefault()
        : null;

    Console.WriteLine($"Region {region} => {(solution.HasValue ? "solved" : "no solution")} {piecePositions}/{boardPositions}={(double)piecePositions / boardPositions:f3} {maxPiecePositions}/{boardPositions}={(double)maxPiecePositions / boardPositions:f3}");

    if (r < 10 && solution.HasValue)
    {
        ConsoleWrite(board, solution.Value);
        Console.WriteLine();
    }

    solutions += solution.HasValue ? 1 : 0;
}

Console.WriteLine($"\nSolutions: {solutions}");

static Piece BuildPiece(int id, bool[,] cells)
{
    var coords = 
        from x in Enumerable.Range(0, cells.GetLength(0))
        from y in Enumerable.Range(0, cells.GetLength(1))
        where cells[x, y]
        select new Coord(x, y);

    ConsoleColor[] colors = [ConsoleColor.DarkRed, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta];

    return PieceBuilder
        .Create(id.ToPieceName())
        .WithPositions(coords)
        .WithOrientations(PieceOrientation.RotateAndReflect)
        .WithAttributes(colors[id % colors.Length])
        .BuildPiece();
}

void ConsoleWrite(Board board, Solution solution)
{
    IDictionary<Coord, Piece> layout = solution.Placements.ToLayout();

    Console.ForegroundColor = ConsoleColor.White;
    Console.BackgroundColor = ConsoleColor.Black;

    foreach (int y in board.Bounds.EnumerateY())
    {
        foreach (int x in board.Bounds.EnumerateX())
        {
            layout.TryGetValue((x, y), out Piece? piece);

            Console.BackgroundColor = piece?.Attributes.ConsoleColor ?? ConsoleColor.Black;
            Console.Write(piece?.Name.Substring(0,1) ?? ".");
        }

        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine();
    }
}

public record Region
{
    public Coord Size { get; }
    public IReadOnlyDictionary<string,int> PieceCounts { get; }

    public Region(Coord size, IEnumerable<int> pieceCounts)
    {
        Size = size;
        PieceCounts = pieceCounts
            .Select((c, i) => (Name: i.ToPieceName(), Count: c))
            .ToDictionary(p => p.Name, p => p.Count);
    }

    public override string ToString() => $"{Size}: {String.Join(", ", PieceCounts.Select(p => $"{p.Key}:{p.Value}"))}";
}

internal static class Extensions
{
    public static string ToPieceName(this int id) => ('A' + id).ToString();

    public static bool TryParseShapeHeader(this string s, out int index)
    {
        Match match = Regex.Match(s, @"^([0-9]+):$");
        if (match.Success &&
            int.TryParse(match.Groups[1].Value, out index))
        {
            return true;
        }

        index = default;
        return false;
    }

    public static bool[,] ReadShapeCells(this StreamReader file)
    {
        List<bool[]> cells = [];
        while (!file.EndOfStream)
        {
            string? s = file.ReadLine()?.Trim();
            if (String.IsNullOrEmpty(s)) break;

            if (!s.TryParseShapeRow(out bool[] row))
            {
                throw new Exception($"Invalid shape input: {s}");
            }

            cells.Add(row);
        }
        return cells.ToArray2D();
    }

    public static bool TryParseArea(this string s, out Coord size, out int[] indexes)
    {
        Match match = Regex.Match(s, @"^([0-9]+)x([0-9]+):(\s+([0-9]+))+\s*$");
        if (match.Success &&
            int.TryParse(match.Groups[1].Value, out int x) &&
            int.TryParse(match.Groups[2].Value, out int y) &&
            TryParseIndexes(match.Groups[4].Captures.Select(c => c.Value), out indexes))
        {
            size = (x, y);
            return true;
        }

        size = default;
        indexes = [];
        return false;
    }

    private static bool TryParseIndexes(this IEnumerable<string> captures, out int[] indexes)
    {
        List<int> values = [];
        foreach(var s in captures)
        {
            if (!int.TryParse(s, out int value))
            {
                indexes = [];
                return false;
            }

            values.Add(value);
        }

        indexes = values.ToArray();
        return true;
    }

    public static bool TryParseShapeRow(this string s, out bool[] cells)
    {
        Match match = Regex.Match(s, @"^[#.]+$");
        if (match.Success)
        {
            cells = s.Select(c => c == '#').ToArray();
            return true;
        }

        cells = [];
        return false;
    }

    public static T[,] ToArray2D<T>(this IEnumerable<T[]> source)
    {
        var rows = source.ToList();
        int ys = rows.Count;
        int xs = rows[0].Length;
        T[,] result = new T[xs,ys];
        for (int y = 0; y < ys; y++)
        {
            var xa = rows[y];
            if (xa.Length != xs)
            {
                throw new ArgumentException(nameof(source), "Arrays must have equal length");
            }

            for (int x = 0; x < xs; x++)
            {
                result[y, x] = xa[x];
            }
        }
        return result;
    }
}
