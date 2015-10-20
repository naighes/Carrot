using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class SuccessfulPublishing : IPublishResult
    {
        public readonly String MessageId;
        public readonly Int64 Timestamp;

        private SuccessfulPublishing(String messageId, Int64 timestamp)
        {
            MessageId = messageId;
            Timestamp = timestamp;
        }

        internal static SuccessfulPublishing FromBasicProperties(IBasicProperties properties)
        {
            return new SuccessfulPublishing(properties.MessageId, properties.Timestamp.UnixTime);
        }
    }
}