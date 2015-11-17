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
            if (_log.IsInfoEnabled)
                _log.Info(message);
        }

        public void Warn(String message, Exception exception = null)
        {
            if (!_log.IsWarnEnabled)
                return;

            if (exception == null)
                _log.Warn(message);
            else
                _log.Warn(message, exception);
        }

        public void Error(String message, Exception exception)
        {
            if (_log.IsErrorEnabled)
                _log.Error(message, exception);
        }
    }
}