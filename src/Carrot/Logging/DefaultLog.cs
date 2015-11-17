using System;
using System.Diagnostics;

namespace Carrot.Logging
{
    public class DefaultLog : ILog
    {
        public void Info(String message)
        {
            if (message == null)
                return;

            Debug.WriteLine("[INFO] {0}", new Object[] { message });
        }

        public void Warn(String message, Exception exception = null)
        {
            Debug.WriteLine("[WARN] {0}:{1}",
                            message ?? "an error has occurred",
                            exception == null ? "[unknow]" : exception.Message);
        }

        public void Error(String message, Exception exception)
        {
            Debug.WriteLine("[ERROR] {0}:{1}",
                            message ?? "an error has occurred",
                            exception == null ? "[unknow]" : exception.Message);
        }
    }
}