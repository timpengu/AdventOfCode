using System.Collections;

// TODO: redefine CoordSpan as record struct, but it will be boxed whenever it is enumerated?
internal record CoordSpan(Coord Z, Coord dZ, int Length) : IEnumerable<Coord>
{
    public Coord this[int i] => Z + i * dZ;
    public IEnumerator<Coord> GetEnumerator() => Enumerable.Range(0, Length).Select(i => this[i]).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public override string ToString() => $"{Z}+{dZ}[{Length}]";
}
