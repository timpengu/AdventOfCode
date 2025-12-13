namespace AoC_2025_12.GameModel.Game;

public record struct Solution(IReadOnlyCollection<Placement> Placements)
{
    public static Solution FromBoard(Board board) => new(board.Placements);

    public override string ToString() => string.Join(", ", Placements);
}
