using System;
using global::log4net;

namespace Carrot.log4net
{
    public class LogWrapper : Logging.ILog
    {
        private readonly ILog _log;

        public LogWrapper(ILog log)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            _log = log;
        }

        public void Info(String message)
        {
            _log.Info(message);
        }

        public void Error(String message, Exception exception)
        {
            _log.Error(message, exception);
        }
    }
}