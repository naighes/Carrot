using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class OutboundMessageEnvelope
    {
        internal OutboundMessageEnvelope(IBasicProperties properties,
                                         UInt64 tag,
                                         Byte[] body)
        {
            Properties = properties;
            Tag = tag;
            Body = body;
        }

        public IBasicProperties Properties { get; }

        public UInt64 Tag { get; }

        public Byte[] Body { get; }
    }
}