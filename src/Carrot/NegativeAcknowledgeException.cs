using System;
using Carrot.Messages;

namespace Carrot
{
    public class NegativeAcknowledgeException : Exception
    {
        internal NegativeAcknowledgeException(IMessage source, String message)
            : base(message)
        {
            SourceMessage = source;
        }

        public IMessage SourceMessage { get; }
    }
}