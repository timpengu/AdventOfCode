record struct State(Coord Z, Coord dZ)
{
    public static implicit operator State((Coord Z, Coord dZ) tuple) => new State(tuple.Z, tuple.dZ);
    public override string ToString() => $"{Z}+{dZ}";
}
