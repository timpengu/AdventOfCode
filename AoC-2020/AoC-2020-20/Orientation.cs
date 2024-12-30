record Orientation(string Name, Coord Origin, Coord dX, Coord dY)
{
    // Orientations are named by their topmost side (Up,Right,Down,Left) and edge vector sense
    public static IEnumerable<Orientation> GetAll()
    {
        yield return Ua;
        yield return Ra;
        yield return Da;
        yield return La;
        yield return Ub;
        yield return Rb;
        yield return Db;
        yield return Lb;
    }

    // Clockwise edge vectors
    public static readonly Orientation Ua = new("Ua", (0, 0), (1, 0), (0, 1));
    public static readonly Orientation Ra = new("Ra", (1, 0), (0, 1), (-1, 0));
    public static readonly Orientation Da = new("Da", (1, 1), (-1, 0), (0, -1));
    public static readonly Orientation La = new("La", (0, 1), (0, -1), (1, 0));

    // Anticlockwise edge vectors
    public static readonly Orientation Ub = new("Ub", (1, 0), (-1, 0), (0, 1));
    public static readonly Orientation Rb = new("Rb", (1, 1), (0, -1), (-1, 0));
    public static readonly Orientation Db = new("Db", (0, 1), (1, 0), (0, -1));
    public static readonly Orientation Lb = new("Lb", (0, 0), (0, 1), (1, 0));

    public override string ToString() => Name;
}
