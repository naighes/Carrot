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
            _logger.Info(message);
        }
    }
}