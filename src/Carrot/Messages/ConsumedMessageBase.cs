using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TowerBridge.Common.Infrastructure.Messaging;

namespace TowerBridge.Common.Infrastructure.Messages
{
    public abstract class ConsumedMessageBase
    {
        protected readonly String MessageId;
        protected readonly UInt64 DeliveryTag;
        protected readonly Boolean Redelivered;

        protected ConsumedMessageBase(String messageId, UInt64 deliveryTag, Boolean redelivered)
        {
            MessageId = messageId;
            DeliveryTag = deliveryTag;
            Redelivered = redelivered;
        }

        internal abstract Task<IAggregateConsumingResult> Consume(IDictionary<Type, IConsumer> subscriptions);
    }
}