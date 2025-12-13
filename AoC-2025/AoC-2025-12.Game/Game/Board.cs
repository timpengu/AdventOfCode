using AoC_2025_12.GameModel.Coords;
using System.Text;

namespace AoC_2025_12.GameModel.Game;

public sealed record Board
{
    private readonly IEnumerable<Placement> _placements;
    private readonly bool[,] _occupation;

    /// <summary>
    /// Returns an empty board of the given size
    /// </summary>
    public static Board Create(Coord size) => new Board(size);

    /// <summary>
    /// Returns a board with additional occupied positions
    /// </summary>
    public Board WithOccupiedPositions(IEnumerable<Coord> positions) => new Board(_placements, WithOccupation(positions));

    /// <summary>
    /// Returns a board with placement of an oriented piece
    /// </summary>
    public Board WithPlacement(Placement placement) => new Board(_placements.Append(placement), WithOccupation(placement.Positions));

    public Coord Size => new(_occupation.GetLength(0), _occupation.GetLength(1));
    public CoordRange Bounds => new(Coord.Zero, Size);

    public IReadOnlyCollection<Placement> Placements => _placements.ToList();

    public bool IsOccupied(Placement placement) => placement.Positions.Any(IsOccupied);
    public bool IsOccupied(Coord position) => _occupation[position.X, position.Y];

    private Board(Coord size)
    {
        if (size.X < 0 || size.Y < 0)
            throw new ArgumentException("Board size cannot be negative.", nameof(size));

        _placements = Enumerable.Empty<Placement>();
        _occupation = new bool[size.X, size.Y];
    }

    private Board(IEnumerable<Placement> placements, bool[,] occupation)
    {
        _placements = placements;
        _occupation = occupation;
    }

    private bool[,] WithOccupation(IEnumerable<Coord> positions)
    {
        bool[,] occupation = (bool[,])_occupation.Clone();
        foreach (Coord position in positions)
        {
            Validate(position);
            occupation[position.X, position.Y] = true;
        }
        return occupation;
    }

    private void Validate(Coord position)
    {
        if (position.X < 0 || position.X >= Size.X ||
            position.Y < 0 || position.Y >= Size.Y)
        {
            throw new IndexOutOfRangeException($"Position {position} is outside board bounds {Bounds}");
        }
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (int y in Bounds.EnumerateY())
        {
            foreach (int x in Bounds.EnumerateX())
            {
                if (x != Bounds.Start.X)
                {
                    sb.Append(' ');
                }
                else if (y != Bounds.Start.Y)
                {
                    sb.AppendLine();
                }

                sb.Append(
                    IsOccupied(new Coord(x, y)) ? 'x' : '.'
                );
            }
        }

        return sb.ToString();
    }
}
