using AoC_2025_12.GameModel.Coords;

namespace AoC_2025_12.GameModel.Game;

public interface IHasPositions
{
    public IEnumerable<Coord> Positions { get; }
}
