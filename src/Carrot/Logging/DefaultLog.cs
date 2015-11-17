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
    }
}