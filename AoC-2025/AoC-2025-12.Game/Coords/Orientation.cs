namespace AoC_2025_12.GameModel.Coords;

public enum Orientation
{
    Ar, // original (right-handed)
    Br, // rotated 90
    Cr, // rotated 180
    Dr, // rotated 270

    Al, // reflected (left-handed)
    Bl, // reflected rotated 90
    Cl, // reflected rotated 180
    Dl, // reflected rotated 270
}
