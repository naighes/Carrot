using Carrot.Messaging;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    // TODO: have to look at something like "media formatters".
    public class OutboundMessage<TMessage> : IMessage<TMessage>
        where TMessage : class
    {
        private readonly TMessage _content;
        private readonly HeaderCollection _headers = new HeaderCollection();

        public OutboundMessage(TMessage content)
        {
            _content = content;
        }

        public HeaderCollection Headers
        {
            get { return _headers; }
        }

        public TMessage Content
        {
            get { return _content; }
        }

        internal virtual void HydrateProperties(IBasicProperties properties)
        {
            Headers.HydrateProperties(properties);
        }
    }
}