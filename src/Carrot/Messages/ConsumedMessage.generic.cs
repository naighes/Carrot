using System;

namespace Carrot.Messages
{
    public class ConsumedMessage<TMessage> : Message<TMessage>
        where TMessage : class
    {
        internal ConsumedMessage(TMessage content, HeaderCollection headers, String consumerTag)
        {
            Content = content;
            Headers = headers;
            ConsumerTag = consumerTag;
        }

        public override TMessage Content { get; }

        public override HeaderCollection Headers { get; }

        public String ConsumerTag { get; }
    }
}