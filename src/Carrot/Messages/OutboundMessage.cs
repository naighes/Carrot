using Carrot.Messaging;

namespace Carrot.Messages
{
    public class OutboundMessage<TMessage> : IMessage<TMessage> where TMessage : class
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
    }
}