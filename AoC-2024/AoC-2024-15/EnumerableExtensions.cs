static class EnumerableExtensions
{
    public static IEnumerable<T> ConcatIfNoneEmpty<T>(this IEnumerable<IEnumerable<T>> sources)
    {
        List<IEnumerator<T>> enumerators = sources
            .Select(source => source.GetEnumerator())
            .ToList();

        // return empty sequence if any source sequence is empty
        foreach (var e in enumerators)
        {
            if (!e.MoveNext())
                yield break;
        }

        // otherwise concatenate all sequences
        foreach (var e in enumerators)
        {
            do
            {
                yield return e.Current;
            }
            while (e.MoveNext());
        }
    }
}