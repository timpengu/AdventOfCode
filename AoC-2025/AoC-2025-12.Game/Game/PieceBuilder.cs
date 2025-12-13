using AoC_2025_12.GameModel.Coords;
using System.Drawing;

namespace AoC_2025_12.GameModel.Game;

public sealed class PieceBuilder
{
    private string _name;

    private PieceAttributes _attributes = new();    
    private IList<Coord> _positions = [];
    private IList<Orientation> _orientations = [];

    public static PieceBuilder Create(string name) => new(name);

    private PieceBuilder(string name)
    {
        _name = name;
    }

    public PieceBuilder WithAttributes(
        ConsoleColor? consoleColor = null,
        Color? htmlColor = null)
    {
        _attributes = _attributes with
        {
            ConsoleColor = consoleColor ?? _attributes.ConsoleColor,
            HtmlColor = htmlColor ?? _attributes.HtmlColor
        };
        return this;
    }

    public PieceBuilder WithPositions(IEnumerable<Coord> positions)
    {
        _positions = positions.ToList();
        return this;
    }

    public PieceBuilder WithOrientations(PieceOrientation pieceOrientation)
    {
        _orientations = pieceOrientation switch
        {
            PieceOrientation.Original => Orientations.Original,
            PieceOrientation.Rotate => Orientations.Rotations,
            PieceOrientation.RotateAndReflect => Orientations.RotationsAndReflections,
            _ => throw new ArgumentException($"Invalid {nameof(PieceOrientation)}: {pieceOrientation}", nameof(pieceOrientation))
        };
        return this;
    }

    public Piece BuildPiece()
    {
        Piece piece = new(_name, _attributes);
        
        piece.AddOrientations(
            _orientations
                .Select(orientation => BuildOrientedPiece(piece, orientation))
                .Distinct(new PositionsComparer<OrientedPiece>()) // exclude duplicate orientations due to symmetry
        );
        
        return piece;
    }

    private OrientedPiece BuildOrientedPiece(Piece piece, Orientation orientation)
    {
        return new OrientedPiece(
            piece,
            orientation,
            _positions.Transform(orientation)
        );
    }
}
