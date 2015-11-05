using System;

namespace Carrot.Messages
{
    public class ConsumedMessage<TMessage> : Message<TMessage>
        where TMessage : class
    {
        private readonly TMessage _content;
        private readonly HeaderCollection _headers;
        private readonly String _consumerTag;

        internal ConsumedMessage(TMessage content, HeaderCollection headers, String consumerTag)
        {
            _content = content;
            _headers = headers;
            _consumerTag = consumerTag;
        }

        public override TMessage Content
        {
            get { return _content; }
        }

        public override HeaderCollection Headers
        {
            get { return _headers; }
        }

        public String ConsumerTag
        {
            get { return _consumerTag; }
        }
    }
}