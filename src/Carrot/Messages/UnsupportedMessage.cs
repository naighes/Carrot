using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Messages
{
    public class UnsupportedMessage : ConsumedMessageBase
    {
        public UnsupportedMessage(String messageId, UInt64 deliveryTag, Boolean redelivered)
            : base(messageId, deliveryTag, redelivered)
        {
        }

        internal override Task<IAggregateConsumingResult> Consume(IDictionary<Type, IConsumer> subscriptions)
        {
            return Task.FromResult((IAggregateConsumingResult)new UnsupportedMessageFailure());
        }
    }

    public class UnsupportedMessageFailure : Failure
    {
        internal UnsupportedMessageFailure()
        {
        }
    }
}