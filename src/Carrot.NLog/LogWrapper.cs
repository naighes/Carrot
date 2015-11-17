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

        public void Error(String message, Exception exception)
        {
            if (_logger.IsErrorEnabled)
                _logger.Error(exception, message);
        }
    }
}