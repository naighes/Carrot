using System;

namespace Carrot.Logging
{
    public interface ILog
    {
        void Info(String message);

        void Error(String message, Exception exception);
    }
}