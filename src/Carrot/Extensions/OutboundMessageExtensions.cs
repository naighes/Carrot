using System;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot.Extensions
{
    internal static class OutboundMessageExtensions
    {
        internal static OutboundMessageEnvelope<TMessage> BuildEnvelope<TMessage>(this OutboundMessage<TMessage> source,
                                                                                  IModel model,
                                                                                  IBasicProperties properties,
                                                                                  Byte[] body,
                                                                                  Exchange exchange,
                                                                                  String routingKey)
            where TMessage : class
        {
            var tag = model.NextPublishSeqNo;
            return new OutboundMessageEnvelope<TMessage>(properties,
                                                         body,
                                                         exchange,
                                                         routingKey,
                                                         source,
                                                         tag);
        }
    }
}