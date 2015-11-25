using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class OutboundMessageEnvelope
    {
        internal OutboundMessageEnvelope(IBasicProperties properties,
                                         Byte[] body,
                                         Exchange exchange,
                                         String routingKey,
                                         UInt64 tag)
        {
            Properties = properties;
            Body = body;
            Exchange = exchange;
            RoutingKey = routingKey;
            Tag = tag;
        }

        public IBasicProperties Properties { get; }

        public Byte[] Body { get; }

        public UInt64 Tag { get; }

        public Exchange Exchange { get; }

        public String RoutingKey { get; }
    }
}