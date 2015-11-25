using System;
using Carrot.Messages;

namespace Carrot
{
    public class MessageNotConfirmedException : Exception
    {
        internal MessageNotConfirmedException(IMessage source, String message)
            : base(message)
        {
            SourceMessage = source;
        }

        public IMessage SourceMessage { get; }
    }
}