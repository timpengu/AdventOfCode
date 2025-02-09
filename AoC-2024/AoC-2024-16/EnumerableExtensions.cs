﻿static class EnumerableExtensions
{
    public static IEnumerable<TSource> WhereMinBy<TSource, TValue>(this IEnumerable<TSource> source, Func<TSource,TValue> selector)
        where TValue : IEquatable<TValue>
    {
        List<TSource> items = source.ToList();
        TValue? minValue = items.Min(selector);
        return items.Where(node => selector(node).Equals(minValue));
    }
}