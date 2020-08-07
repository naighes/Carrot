using Carrot.Configuration;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class DurableOutboundMessage<TMessage> : OutboundMessage<TMessage>
        where TMessage : class
    {
        public DurableOutboundMessage(TMessage content)
            : base(content)
        {
        }

        internal override IBasicProperties BuildBasicProperties(IBasicProperties basicProperties,
                                                                IMessageTypeResolver resolver,
                                                                IDateTimeProvider dateTimeProvider,
                                                                INewId idGenerator)
        {
            var properties = base.BuildBasicProperties(basicProperties, resolver, dateTimeProvider, idGenerator);
            properties.Persistent = true;
            return properties;
        }
    }
}