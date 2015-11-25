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
                                         UInt64 tag,
                                         OutboundMessage<TMessage> source)
        {
            Properties = properties;
            Body = body;
            Exchange = exchange;
            RoutingKey = routingKey;
            Tag = tag;
            Source = source;
        }

        public IBasicProperties Properties { get; }

        public Byte[] Body { get; }

        public Exchange Exchange { get; }

        public String RoutingKey { get; }

        public UInt64 Tag { get; }

        public OutboundMessage<TMessage> Source { get; set; }
    }
}