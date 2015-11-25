using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class OutboundMessageEnvelope<TMessage>
        where TMessage : class
    {
        internal OutboundMessageEnvelope(IBasicProperties properties,
                                         Byte[] body,
                                         Exchange exchange,
                                         String routingKey,
                                         OutboundMessage<TMessage> source,
                                         UInt64 tag)
        {
            Properties = properties;
            Body = body;
            Exchange = exchange;
            RoutingKey = routingKey;
            Source = source;
            Tag = tag;
        }

        public IBasicProperties Properties { get; }

        public Byte[] Body { get; }

        public Exchange Exchange { get; }

        public String RoutingKey { get; }

        public OutboundMessage<TMessage> Source { get; set; }

        public UInt64 Tag { get; }
    }
}