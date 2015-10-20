using System;

namespace Carrot
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow();
    }
}