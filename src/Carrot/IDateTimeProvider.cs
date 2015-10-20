using System;

namespace Carrot
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
    }

    internal class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}