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

        internal override void HydrateProperties(IBasicProperties properties)
        {
            base.HydrateProperties(properties);
            properties.Persistent = true;
        }
    }
}