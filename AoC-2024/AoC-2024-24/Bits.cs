using MoreLinq;

static class Bits
{
    public const int Maximum = 45;
    public const ulong OverflowValue = 1ul << Maximum;
    public static IEnumerable<ulong> GetBitValues() => GetBitIndexes().Select(ToBitValue);
    public static IEnumerable<int> GetBitIndexes() => MoreEnumerable.Sequence(0, Maximum);
    public static ulong ToBitValue(this int bit) => 1ul << bit;
    public static int ToInt(this bool value) => value ? 1 : 0;
}
