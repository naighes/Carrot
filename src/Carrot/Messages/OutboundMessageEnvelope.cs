using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    internal class OutboundMessageEnvelope
    {
        internal OutboundMessageEnvelope(IBasicProperties properties,
                                         UInt64 tag,
                                         Byte[] body)
        {
            Properties = properties;
            Tag = tag;
            Body = body;
        }

        internal IBasicProperties Properties { get; }

        internal UInt64 Tag { get; }

        internal Byte[] Body { get; }
    }
}