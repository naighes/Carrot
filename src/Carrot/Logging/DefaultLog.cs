using System;

namespace Carrot.Logging
{
    public class DefaultLog : ILog
    {
        public void Info(String message)
        {
            if (message == null)
                return;

            System.Diagnostics.Debug.WriteLine("[INFO] {0}", new Object[] { message });
        }

        public void Warn(String message, Exception exception = null)
        {
            System.Diagnostics.Debug.WriteLine("[WARN] {0}:{1}",
                                               message ?? "an error has occurred",
                                               exception?.Message ?? "[unknow]");
        }

        public void Error(String message, Exception exception = null)
        {
            System.Diagnostics.Debug.WriteLine("[ERROR] {0}:{1}",
                                               message ?? "an error has occurred",
                                               exception?.Message ?? "[unknow]");
        }

        public void Fatal(String message, Exception exception = null)
        {
            System.Diagnostics.Debug.WriteLine("[FATAL] {0}:{1}",
                                               message ?? "an error has occurred",
                                               exception?.Message ?? "[unknow]");
        }

        public void Debug(String message)
        {
            if (message == null)
                return;

            System.Diagnostics.Debug.WriteLine("[DEBUG] {0}", new Object[] { message });
        }
    }
}