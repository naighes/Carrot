using Carrot.Configuration;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class OutboundMessage<TMessage> : Message<TMessage>
        where TMessage : class
    {
        public OutboundMessage(TMessage content)
        {
            Content = content;
        }

        public override HeaderCollection Headers { get; } = new OutboundHeaderCollection();

        public override TMessage Content { get; }

        internal virtual IBasicProperties BuildBasicProperties(IMessageTypeResolver resolver,
                                                               IDateTimeProvider dateTimeProvider,
                                                               INewId idGenerator)
        {
            return ((OutboundHeaderCollection)Headers).BuildBasicProperties<TMessage>(idGenerator,
                                                                                      dateTimeProvider,
                                                                                      resolver);
        }
    }
}