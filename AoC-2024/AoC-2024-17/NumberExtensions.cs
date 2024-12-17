static class NumberExtensions
{
    public static string ToOctalString(this long value) => $"0{Convert.ToString(value, 8)}";
}
