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

            Debug.WriteLine(message);
        }

        public void Error(String message, Exception exception)
        {
            Debug.WriteLine("{0}:{1}",
                            message ?? "an error has occurred",
                            exception == null ? "[unknow]" : exception.Message);
        }
    }
}