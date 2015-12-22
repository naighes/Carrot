using System;
using System.Collections.Generic;

namespace Carrot.Benchmarks.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TResult> Repeat<TResult>(this IEnumerable<TResult> source, Int32 count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            for (var i = 0; i < count; i++)
                foreach (var result in source)
                    yield return result;
        }
    }
}