using System;
using global::log4net;

namespace Carrot.log4net
{
    public class LogWrapper : Logging.ILog
    {
        private readonly ILog _log;

        public LogWrapper(ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
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
            if (!_log.IsErrorEnabled)
                return;

            if (exception == null)
                _log.Error(message);
            else
                _log.Error(message, exception);
        }

        public void Fatal(String message, Exception exception = null)
        {
            if (!_log.IsFatalEnabled)
                return;

            if (exception == null)
                _log.Fatal(message);
            else
                _log.Fatal(message, exception);
        }

        public void Debug(String message)
        {
            if (_log.IsDebugEnabled)
                _log.Debug(message);
        }
    }
}