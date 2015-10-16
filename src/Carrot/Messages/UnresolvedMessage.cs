using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TowerBridge.Common.Infrastructure.Messaging;

namespace TowerBridge.Common.Infrastructure.Messages
{
    public class UnresolvedMessage : ConsumedMessageBase
    {
        public UnresolvedMessage(String messageId, UInt64 deliveryTag, Boolean redelivered)
            : base(messageId, deliveryTag, redelivered)
        {
        }

        internal override Task<IAggregateConsumingResult> Consume(IDictionary<Type, IConsumer> subscriptions)
        {
            return Task.FromResult((IAggregateConsumingResult)new Failure());
        }
    }
}