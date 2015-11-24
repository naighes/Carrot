using System;

namespace Carrot.Extensions
{
    internal static class DateTimeOffsetExtensions
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        internal static Int64 ToUnixTimestamp(this DateTimeOffset value)
        {
            return (Int64)(value - Epoch).TotalSeconds;
        }

        internal static DateTimeOffset ToDateTimeOffset(this Int64 value)
        {
            return Epoch.Add(TimeSpan.FromSeconds(value));
        }
    }
}