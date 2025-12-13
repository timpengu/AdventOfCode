namespace AoC_2025_12.GameModel;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ThrowIfEmpty<T>(this IEnumerable<T> source) =>
        source.ThrowIfEmpty(() => new InvalidOperationException("Sequence is empty"));

    public static IEnumerable<T> ThrowIfEmpty<T>(this IEnumerable<T> source, Func<Exception> getException)
    {
        var enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw getException();
        }

        do
        {
            yield return enumerator.Current;
        }
        while (enumerator.MoveNext());
    }
}
