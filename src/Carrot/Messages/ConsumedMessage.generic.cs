namespace Carrot.Messages
{
    public class ConsumedMessage<TMessage> : Message<TMessage>
        where TMessage : class
    {
        private readonly TMessage _content;
        private readonly HeaderCollection _headers;

        internal ConsumedMessage(TMessage content, HeaderCollection headers)
        {
            _content = content;
            _headers = headers;
        }

        public override TMessage Content
        {
            get { return _content; }
        }

        public override HeaderCollection Headers
        {
            get { return _headers; }
        }
    }
}