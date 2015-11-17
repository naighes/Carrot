using System;
using Carrot.Logging;
using global::NLog;

namespace Carrot.NLog
{
    public class LogWrapper : ILog
    {
        private readonly ILogger _logger;

        public LogWrapper(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _logger = logger;
        }

        public void Info(String message)
        {
            if (_logger.IsInfoEnabled)
                _logger.Info(message);
        }

        public void Warn(String message, Exception exception = null)
        {
            if (!_logger.IsWarnEnabled)
                return;

            if (exception == null)
                _logger.Warn(message);
            else
                _logger.Warn(exception, message);
        }

        public void Error(String message, Exception exception = null)
        {
            if (!_logger.IsErrorEnabled)
                return;

            if (exception == null)
                _logger.Error(message);
            else
                _logger.Error(exception, message);
        }

        public void Fatal(String message, Exception exception = null)
        {
            if (!_logger.IsFatalEnabled)
                return;

            if (exception == null)
                _logger.Fatal(message);
            else
                _logger.Fatal(exception, message);
        }

        public void Debug(String message)
        {
            if (_logger.IsDebugEnabled)
                _logger.Debug(message);
        }
    }
}