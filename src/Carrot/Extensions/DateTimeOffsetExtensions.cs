using System;

namespace Carrot.Extensions
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        public static Int64 ToUnixTimestamp(this DateTimeOffset value)
        {
            return (Int64)(value - Epoch).TotalSeconds;
        }
    }
}