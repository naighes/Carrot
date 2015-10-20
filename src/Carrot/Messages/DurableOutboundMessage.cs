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

        internal override IBasicProperties ToOutboundBasicProperties()
        {
            var properties = base.ToOutboundBasicProperties();
            properties.Persistent = true;

            return properties;
        }
    }
}