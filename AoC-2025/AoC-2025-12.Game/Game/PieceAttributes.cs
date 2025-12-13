using System.Drawing;

namespace AoC_2025_12.GameModel.Game;

public sealed record PieceAttributes(
    ConsoleColor ConsoleColor = default,
    Color HtmlColor = default)
{
}
