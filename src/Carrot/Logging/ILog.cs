using System;

namespace Carrot.Logging
{
    public interface ILog
    {
        void Info(String message);

        void Warn(String message, Exception exception = null);

        void Error(String message, Exception exception);
    }
}