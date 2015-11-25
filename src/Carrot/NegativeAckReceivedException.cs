using System;
using Carrot.Messages;

namespace Carrot
{
    public class NegativeAckReceivedException : Exception
    {
        internal NegativeAckReceivedException(IMessage source, String message)
            : base(message)
        {
            SourceMessage = source;
        }

        public IMessage SourceMessage { get; }
    }
}