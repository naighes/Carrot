using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class OutboundMessage<TMessage> : Message<TMessage>
        where TMessage : class
    {
        private readonly TMessage _content;
        private readonly HeaderCollection _headers = new HeaderCollection();

        public OutboundMessage(TMessage content)
        {
            _content = content;
        }

        public override HeaderCollection Headers
        {
            get { return _headers; }
        }

        public override TMessage Content
        {
            get { return _content; }
        }

        internal virtual void HydrateProperties(IBasicProperties properties)
        {
            Headers.HydrateProperties(properties);
        }
    }
}