namespace AoC_2025_12.GameModel.Game;

public sealed class Piece
{
    public static Piece OccupiedPosition = new(".");

    internal Piece(string name, PieceAttributes? attributes = null)
    {
        Name = name;
        Attributes = attributes ?? new();
    }

    internal void AddOrientations(IEnumerable<OrientedPiece> orientations)
    {
        _orientations.AddRange(orientations.ThrowIfEmpty());
        PositionCount = _orientations.Select(o => o.Positions.Count).Distinct().Single(); // all orientations must have same positions count
    }

    public string Name { get; }
    public PieceAttributes Attributes { get; }

    public IReadOnlyList<OrientedPiece> Orientations => _orientations;
    private List<OrientedPiece> _orientations = [];

    public int PositionCount { get; private set; } = 0;

    public override string ToString() => Name;
}
