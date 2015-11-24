using System.Collections.Generic;
using System.Linq;

namespace Carrot.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> NotNull<T>(this IEnumerable<T> source) where T : class
        {
            return source.Where(_ => _ != null);
        }
    }
}