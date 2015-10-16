using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TowerBridge.Common.Infrastructure.Messaging;

namespace TowerBridge.Common.Infrastructure.Messages
{
    public class CorruptedMessage : ConsumedMessageBase
    {
        public CorruptedMessage(String messageId, UInt64 deliveryTag, Boolean redelivered)
            : base(messageId, deliveryTag, redelivered)
        {
        }

        internal override Task<IAggregateConsumingResult> Consume(IDictionary<Type, IConsumer> subscriptions)
        {
            return Task.FromResult((IAggregateConsumingResult)new CorruptedMessageFailure());
        }
    }

    public class CorruptedMessageFailure : Failure
    {
        internal CorruptedMessageFailure()
        {
        }
    }
}